import {
    Component,
    EventEmitter,
    Input,
    Output,
    inject,
    signal,
    computed,
    DestroyRef,
    effect
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
    FormBuilder,
    Validators,
    ReactiveFormsModule,
    FormArray,
    AbstractControl
} from '@angular/forms';
import { TreeSelectModule } from 'primeng/treeselect';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
    forkJoin,
    of,
    finalize
} from 'rxjs';

import { OrganizationService } from '../../../services/organisation/organisation.service';
import { PersonnelService } from '../../../services/organisation/personnel.service';
import { UserService } from '../../../services/user/user.service';
import { NotificationService } from '../../../services/notification.service';
import { ValidationService } from '../../../services/user/validation.service';

import {
    CreatePersonnelRequest,
    CreateAffectationRequest,
    StatutPersonnel,
    PrimeNgTreeNode
} from '../../../models/organisation/personnel.model';
import { OrganizationTreeNode, TypeEntite } from '../../../models/organisation/organisation-model';

@Component({
    selector: 'app-personnel-create',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, TreeSelectModule],
    templateUrl: './personnel-create.component.html',
    styleUrl: './personnel-create.component.scss'
})
export class PersonnelCreateComponent {

//#region ─────────────────── INJECTIONS & ETATS ───────────────────
    private readonly fb = inject(FormBuilder);
    private readonly personnelService = inject(PersonnelService);
    private readonly organisationService = inject(OrganizationService);
    private readonly userService = inject(UserService);
    private readonly notification = inject(NotificationService);
    public readonly valService = inject(ValidationService);
    private readonly destroyRef = inject(DestroyRef);

    readonly loading = signal(false);
    readonly currentStep = signal(1);
    readonly organizationTree = signal<OrganizationTreeNode[]>([]);
    readonly fonctionsTree = signal<PrimeNgTreeNode[]>([]);
    readonly statuts = signal<StatutPersonnel[]>([]);
    readonly roles = signal<any[]>([]);
    // Signal to track form changes for reactive validation
    private readonly formStatusChanged = signal(0);

    @Output() cancel = new EventEmitter<void>();
    @Output() personnelSaved = new EventEmitter<void>();

    // ───────────────────────── FORMULAIRE TYPÉ
    readonly form = this.fb.nonNullable.group({
        // Step 1: Personnel information
        matricule: ['', Validators.required],
        nom: ['', Validators.required],
        prenom: ['', Validators.required],
        dateRecrutement: [''],
        dateNaissance: [''],
        email: ['', [Validators.required, Validators.email]],
        statut: [''],
        grade: [''],
        entiteId: [null as OrganizationTreeNode | null, Validators.required],
        createUser: [false],
        userRole: [''],
        // Step 2: Affectations
        affectations: this.fb.array([])
    });

    get affectationsFormArray(): FormArray {
        return this.form.get('affectations') as FormArray;
    }

    readonly isStep1Valid = computed(() => {
        // Depend on formStatusChanged to trigger updates
        this.formStatusChanged();
        const step1Controls = ['matricule', 'nom', 'prenom', 'email', 'entiteId'];
        return step1Controls.every(controlName => {
            const control = this.form.get(controlName);
            return control && control.valid;
        });
    });

    readonly isStep2Valid = computed(() => {
        // Depend on formStatusChanged to trigger updates
        this.formStatusChanged();
        return this.affectationsFormArray.length > 0 && this.affectationsFormArray.valid;
    });

    readonly canGoNext = computed(() => {
        switch (this.currentStep()) {
            case 1:
                return this.isStep1Valid();
            case 2:
                return this.isStep2Valid();
            default:
                return true;
        }
    });

    readonly shouldShowBackButton = computed(() => this.currentStep() > 1);

    readonly isFormValid = computed(() => {
        return this.form.valid && this.affectationsFormArray.length > 0;
    });

//#endregion

//#region  ───────────────────────── LIFECYCLE & INIT ──────
    constructor() {
        this.setupDataLoadingEffect();
        this.setupFormStatusTracking();
        this.setupUserRoleToggle();
    }

    private setupDataLoadingEffect(): void {
        effect(() => {
            this.loadData();
        }, { allowSignalWrites: true });
    }

    private setupFormStatusTracking(): void {
        // Subscribe to form value and status changes to update formStatusChanged signal
        this.form.valueChanges
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe(() => {
                // Trigger the signal update to notify computed properties
                this.formStatusChanged.update(v => v + 1);
            });
    }

    private setupUserRoleToggle(): void {
        // Enable/disable userRole control based on createUser checkbox
        this.form.get('createUser')?.valueChanges
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe((isEnabled: boolean) => {
                const userRoleControl = this.form.get('userRole');
                if (userRoleControl) {
                    if (isEnabled) {
                        userRoleControl.enable();
                    } else {
                        userRoleControl.disable();
                        userRoleControl.reset();
                    }
                }
            });
    }

    private loadData(): void {
        this.loading.set(true);

        forkJoin({
            organizationTree: this.organisationService.getOrganizationTree(),
            fonctionsTree: this.personnelService.getFonctionsTree(),
            statuts: this.personnelService.getStatutPersonnel(),
            roles: this.userService.getRoles()
        })
            .pipe(
                takeUntilDestroyed(this.destroyRef),
                finalize(() => this.loading.set(false))
            )
            .subscribe(({ organizationTree, fonctionsTree, statuts, roles }) => {
                const tree = this.organisationService.addKeysToTree(organizationTree);
                this.organizationTree.set(tree);
                this.fonctionsTree.set(fonctionsTree);
                this.statuts.set(statuts);
                this.roles.set(roles || []);

                // Initialize form with at least one empty affectation
                queueMicrotask(() => {
                    this.currentStep.set(1);
                    if (this.affectationsFormArray.length === 0) {
                        this.addAffectation();
                    }
                    this.form.reset({
                        matricule: '',
                        nom: '',
                        prenom: '',
                        dateRecrutement: '',
                        dateNaissance: '',
                        email: '',
                        statut: '',
                        grade: '',
                        entiteId: null,
                        createUser: false,
                        userRole: '',
                        affectations: this.affectationsFormArray.getRawValue()
                    });
                });
            });
    }

//#endregion

//#region ────────────────────ÉTAPES ET NAVIGATION ────────────────────────
    nextStep(): void {
        if (this.currentStep() === 1 && !this.isStep1Valid()) {
            this.form.markAllAsTouched();
            return;
        }

        if (this.currentStep() === 2 && !this.isStep2Valid()) {
            this.affectationsFormArray.markAllAsTouched();
            return;
        }

        const nextStep = this.currentStep() + 1;
        if (nextStep <= 3) {
            this.currentStep.set(nextStep);
        }
    }

    previousStep(): void {
        const prevStep = this.currentStep() - 1;
        if (prevStep >= 1) {
            this.currentStep.set(prevStep);
        }
    }

//#endregion

//#region ────────────────────GESTION DES AFFECTATIONS (FormArray) ────────────────────────
    addAffectation(): void {
        const affectationControl = this.fb.group({
            entiteId: [null as OrganizationTreeNode | null, Validators.required],
            fonctionId: [null as PrimeNgTreeNode | null, Validators.required]
        });

        this.affectationsFormArray.push(affectationControl);
    }

    removeAffectation(index: number): void {
        if (this.affectationsFormArray.length > 1) {
            this.affectationsFormArray.removeAt(index);
            this.notification.info('Affectation supprimée');
        } else {
            this.notification.warn('Au moins une affectation est requise');
        }
    }

    getAffectationControl(index: number, controlName: string) {
        return this.affectationsFormArray.at(index).get(controlName);
    }

    getSelectedNodeLabel(control: AbstractControl | null): string {
        const value = control?.value;

        if (!value) {
            return '-';
        }

        if (typeof value === 'string' || typeof value === 'number') {
            return String(value);
        }

        if (typeof value.label === 'string' && value.label.trim()) {
            return value.label;
        }

        if (value.data && typeof value.data === 'object') {
            if (typeof value.data.designation === 'string' && value.data.designation.trim()) {
                return value.data.designation;
            }

            if (typeof value.data.libelle === 'string' && value.data.libelle.trim()) {
                return value.data.libelle;
            }
        }

        return '-';
    }

    getSelectedRoleLabel(): string {
        const selectedRole = this.form.get('userRole')?.value;

        if (!selectedRole) {
            return '-';
        }

        const matchingRole = this.roles().find((role) => {
            const roleId = role?.id ?? role;
            return roleId === selectedRole;
        });

        if (!matchingRole) {
            return String(selectedRole);
        }

        return matchingRole.name || matchingRole.displayName || String(matchingRole.id || matchingRole);
    }

//#endregion

//#region ────────────────────ACTIONS PRINCIPALES ────────────────────────
    onSubmit(): void {
        if (this.loading()) return; // protection double clic

        if (this.form.invalid || this.affectationsFormArray.length === 0) {
            this.form.markAllAsTouched();
            this.affectationsFormArray.markAllAsTouched();
            this.notification.error('Veuillez remplir tous les champs requis');
            return;
        }

        const raw = this.form.getRawValue();
        const { entiteId, createUser, userRole, ...rest } = raw;
        // Extract entiteId from the tree node
        const entiteIdValue = this.extractNodeId(entiteId);
        
        if (!entiteIdValue) {
            this.notification.error('Entité requise');
            return;
        }

        // Transform affectations from live FormArray controls to avoid losing selected values
        const affectationsPayload: CreateAffectationRequest[] = this.affectationsFormArray.controls
            .map((control) => {
                const entiteIdAff = this.extractNodeId(control.get('entiteId')?.value);
                const fonctionIdAff = this.extractNodeId(control.get('fonctionId')?.value);

                if (!entiteIdAff || !fonctionIdAff) {
                    return null;
                }

                return {
                    entiteId: entiteIdAff,
                    fonctionId: fonctionIdAff,
                    dateDebut: new Date().toISOString().split('T')[0],
                    nature: 'PERMANENTE' // Default nature
                };
            })
            .filter((aff): aff is CreateAffectationRequest => aff !== null);

        if (affectationsPayload.length === 0) {
            this.notification.error('Au moins une affectation valide est requise');
            return;
        }

        // Only include userRole if createUser is true
        const finalUserRole = createUser && userRole ? userRole : undefined;

        const payload: CreatePersonnelRequest = {
            ...rest,
            entiteId: entiteIdValue,
            createUser: createUser ?? false,
            userRole: finalUserRole,
            affectations: affectationsPayload
        };

        this.loading.set(true);

        this.personnelService.createPersonnel(payload)
            .pipe(
                takeUntilDestroyed(this.destroyRef),
                finalize(() => this.loading.set(false))
            )
            .subscribe(() => {
                this.notification.success('Personnel créé avec succès');
                this.personnelSaved.emit();
                this.onCancel();
            });
    }

    private extractNodeId(node: unknown): number | null {
        if (node == null) {
            return null;
        }

        if (typeof node === 'number') {
            return Number.isNaN(node) ? null : node;
        }

        if (typeof node === 'string') {
            const parsed = this.parseIdValue(node);
            return parsed;
        }

        if (typeof node !== 'object') {
            return null;
        }

        const treeNode = node as {
            key?: string | number | null;
            id?: string | number | null;
            data?: unknown;
        };

        if (treeNode.key != null) {
            const parsed = this.parseIdValue(treeNode.key);
            return parsed;
        }

        if (treeNode.id != null) {
            const parsed = this.parseIdValue(treeNode.id);
            return parsed;
        }

        if (typeof treeNode.data === 'number') {
            return Number.isNaN(treeNode.data) ? null : treeNode.data;
        }

        if (treeNode.data && typeof treeNode.data === 'object') {
            const dataNode = treeNode.data as { id?: string | number | null };

            if (dataNode.id != null) {
                const parsed = this.parseIdValue(dataNode.id);
                return parsed;
            }
        }

        return null;
    }

    private parseIdValue(value: string | number): number | null {
        if (typeof value === 'number') {
            return Number.isNaN(value) ? null : value;
        }

        const directValue = Number(value);
        if (!Number.isNaN(directValue)) {
            return directValue;
        }

        const trailingDigitsMatch = value.match(/(\d+)$/);
        if (!trailingDigitsMatch) {
            return null;
        }

        const extractedValue = Number(trailingDigitsMatch[1]);
        return Number.isNaN(extractedValue) ? null : extractedValue;
    }

    onCancel(): void {
        this.form.reset();
        // Re-disable the userRole control after reset
        this.form.get('userRole')?.disable();
        this.affectationsFormArray.clear();
        this.currentStep.set(1);
        this.cancel.emit();
    }

//#endregion

//#region ────────────────────UTILITAIRES ────────────────────────
    readonly isUserRoleEnabled = computed(() => {
        // Depend on formStatusChanged to ensure reactive updates
        this.formStatusChanged();
        return this.form.get('createUser')?.value ?? false;
    });

    isFieldInvalid(fieldName: string): boolean {
        return this.valService.isFieldInvalid(this.form, fieldName);
    }

    getErrorMessage(fieldName: string): string {
        return this.valService.getErrorMessage(this.form, fieldName);
    }

//#endregion
}
