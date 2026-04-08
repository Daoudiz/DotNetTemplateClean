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
    AbstractControl,
    FormArray,
    FormBuilder,
    ReactiveFormsModule,
    Validators
} from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable, finalize, forkJoin, of } from 'rxjs';

import { TabsModule } from 'primeng/tabs';
import { TreeSelectModule } from 'primeng/treeselect';

import { OrganizationService } from '../../../services/organisation/organisation.service';
import { PersonnelService } from '../../../services/organisation/personnel.service';
import { UserService } from '../../../services/user/user.service';
import { NotificationService } from '../../../services/notification.service';
import { ValidationService } from '../../../services/user/validation.service';

import {
    CreateAffectationRequest,
    CreatePersonnelRequest,
    PersonnelEditAffectationDto,
    PersonnelDetailsDto,
    PersonnelListDto,
    PrimeNgTreeNode,
    StatutPersonnel,
    UpdateAffectationRequest,
    UpdatePersonnelRequest
} from '../../../models/organisation/personnel.model';
import { OrganizationTreeNode } from '../../../models/organisation/organisation-model';

@Component({
    selector: 'app-personnel-create',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, TreeSelectModule, TabsModule],
    templateUrl: './personnel-create.component.html',
    styleUrl: './personnel-create.component.scss'
})
export class PersonnelCreateComponent {

    private readonly fb = inject(FormBuilder);
    private readonly personnelService = inject(PersonnelService);
    private readonly organisationService = inject(OrganizationService);
    private readonly userService = inject(UserService);
    private readonly notification = inject(NotificationService);
    public readonly valService = inject(ValidationService);
    private readonly destroyRef = inject(DestroyRef);

    private readonly personnelId = signal<number | null>(null);
    private readonly formStatusChanged = signal(0);

    readonly isEditMode = computed(() => this.personnelId() !== null);
    readonly loading = signal(false);
    readonly currentStep = signal(1);
    readonly organizationTree = signal<OrganizationTreeNode[]>([]);
    readonly fonctionsTree = signal<PrimeNgTreeNode[]>([]);
    readonly statuts = signal<StatutPersonnel[]>([]);
    readonly roles = signal<any[]>([]);

    @Output() cancel = new EventEmitter<void>();
    @Output() personnelSaved = new EventEmitter<void>();

    @Input()
    set personnelToEdit(value: PersonnelListDto | null) {
        if (this.loading()) {
            return;
        }

        this.personnelId.set(value?.id ?? null);
    }

    readonly form = this.fb.nonNullable.group({
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
        userRole: [{ value: '', disabled: true }],
        affectations: this.fb.array([])
    });

    get affectationsFormArray(): FormArray {
        return this.form.get('affectations') as FormArray;
    }

    readonly isStep1Valid = computed(() => {
        this.formStatusChanged();
        const step1Controls = ['matricule', 'nom', 'prenom', 'email', 'entiteId'];

        return step1Controls.every((controlName) => {
            const control = this.form.get(controlName);
            return !!control && (control.disabled || control.valid);
        });
    });

    readonly isStep2Valid = computed(() => {
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
        this.formStatusChanged();
        return this.form.valid && this.affectationsFormArray.length > 0;
    });

    readonly isUserRoleEnabled = computed(() => {
        this.formStatusChanged();
        return this.form.get('createUser')?.value ?? false;
    });

    constructor() {
        this.setupDataLoadingEffect();
        this.setupFormStatusTracking();
        this.setupUserRoleToggle();
    }

    private setupDataLoadingEffect(): void {
        effect(() => {
            const id = this.personnelId();
            this.loadData(id);
        }, { allowSignalWrites: true });
    }

    private setupFormStatusTracking(): void {
        this.form.valueChanges
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe(() => {
                this.formStatusChanged.update((value) => value + 1);
            });

        this.form.statusChanges
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe(() => {
                this.formStatusChanged.update((value) => value + 1);
            });
    }

    private setupUserRoleToggle(): void {
        this.form.get('createUser')?.valueChanges
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe((isEnabled: boolean) => {
                if (this.isEditMode()) {
                    return;
                }

                this.toggleUserRoleControl(isEnabled);
            });
    }

    private loadData(id: number | null): void {
        this.loading.set(true);

        forkJoin({
            organizationTree: this.organisationService.getOrganizationTree(),
            fonctionsTree: this.personnelService.getFonctionsTree(),
            statuts: this.personnelService.getStatutPersonnel(),
            roles: this.userService.getRoles(),
            personnel: id ? this.personnelService.getPersonnelById(id) : of(null)
        })
            .pipe(
                takeUntilDestroyed(this.destroyRef),
                finalize(() => this.loading.set(false))
            )
            .subscribe(({ organizationTree, fonctionsTree, statuts, roles, personnel }) => {
                this.organizationTree.set(this.organisationService.addKeysToTree(organizationTree));
                this.fonctionsTree.set(fonctionsTree);
                this.statuts.set(statuts);
                this.roles.set(roles || []);

                queueMicrotask(() => {
                        if (personnel) {
                            this.patchEditForm(personnel);
                        } else {
                            this.resetCreateForm();
                    }
                });
            });
    }

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

    addAffectation(): void {
        this.affectationsFormArray.push(this.createAffectationGroup());
        this.formStatusChanged.update((value) => value + 1);
    }

    removeAffectation(index: number): void {
        if (this.affectationsFormArray.length > 1) {
            this.affectationsFormArray.removeAt(index);
            this.formStatusChanged.update((value) => value + 1);
            this.notification.info('Affectation supprimee');
        } else {
            this.notification.warn('Au moins une affectation est requise');
        }
    }

    getAffectationControl(index: number, controlName: string): AbstractControl | null {
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

    onSubmit(): void {
        if (this.loading()) {
            return;
        }

        if (this.form.invalid || this.affectationsFormArray.length === 0) {
            this.form.markAllAsTouched();
            this.affectationsFormArray.markAllAsTouched();
            this.notification.error('Veuillez remplir tous les champs requis');
            return;
        }

        const request$: Observable<unknown> = this.isEditMode()
            ? this.personnelService.updatePersonnel(this.personnelId()!, this.buildUpdatePayload())
            : this.personnelService.createPersonnel(this.buildCreatePayload());

        this.loading.set(true);

        request$
            .pipe(
                takeUntilDestroyed(this.destroyRef),
                finalize(() => this.loading.set(false))
            )
            .subscribe(() => {
                this.notification.success(
                    this.isEditMode()
                        ? 'Personnel mis a jour avec succes'
                        : 'Personnel cree avec succes'
                );
                this.personnelSaved.emit();
                this.onCancel();
            });
    }

    onCancel(): void {
        this.currentStep.set(1);
        if (this.isEditMode()) {
            this.clearAffectations();
            this.form.reset();
        } else {
            this.resetCreateForm();
        }
        this.cancel.emit();
    }

    isFieldInvalid(fieldName: string): boolean {
        return this.valService.isFieldInvalid(this.form, fieldName);
    }

    getErrorMessage(fieldName: string): string {
        return this.valService.getErrorMessage(this.form, fieldName);
    }

    private buildCreatePayload(): CreatePersonnelRequest {
        const raw = this.form.getRawValue();
        const entiteIdValue = this.extractNodeId(raw.entiteId);

        if (!entiteIdValue) {
            throw new Error('Entite requise');
        }

        const finalUserRole = raw.createUser && raw.userRole ? raw.userRole : undefined;

        return {
            matricule: raw.matricule,
            nom: raw.nom,
            prenom: raw.prenom,
            dateRecrutement: raw.dateRecrutement || null,
            dateNaissance: raw.dateNaissance || null,
            email: raw.email,
            entiteId: entiteIdValue,
            statut: raw.statut || null,
            grade: raw.grade || null,
            createUser: raw.createUser ?? false,
            userRole: finalUserRole,
            affectations: this.buildCreateAffectationsPayload()
        };
    }

    private buildUpdatePayload(): UpdatePersonnelRequest {
        const raw = this.form.getRawValue();

        return {
            matricule: raw.matricule,
            nom: raw.nom,
            prenom: raw.prenom,
            dateRecrutement: raw.dateRecrutement || null,
            dateNaissance: raw.dateNaissance || null,
            statut: raw.statut || null,
            grade: raw.grade || null,
            affectations: this.buildUpdateAffectationsPayload()
        };
    }

    private buildCreateAffectationsPayload(): CreateAffectationRequest[] {
        return this.affectationsFormArray.controls
            .map((control) => {
                const entiteId = this.extractNodeId(control.get('entiteId')?.value);
                const fonctionId = this.extractNodeId(control.get('fonctionId')?.value);

                if (!entiteId || !fonctionId) {
                    return null;
                }

                return {
                    entiteId,
                    fonctionId,
                    dateDebutAffectation: control.get('dateDebutAffectation')?.value || this.getTodayDate(),
                    dateDebut: control.get('dateDebutAffectation')?.value || this.getTodayDate(),
                    nature: control.get('nature')?.value || 'PERMANENTE'
                };
            })
            .filter((affectation): affectation is CreateAffectationRequest => affectation !== null);
    }

    private buildUpdateAffectationsPayload(): UpdateAffectationRequest[] {
        return this.affectationsFormArray.controls
            .map((control) => {
                const entiteId = this.extractNodeId(control.get('entiteId')?.value);
                const fonctionId = this.extractNodeId(control.get('fonctionId')?.value);

                if (!entiteId || !fonctionId) {
                    return null;
                }

                return {
                    id: Number(control.get('id')?.value || 0),
                    entiteId,
                    fonctionId,
                    dateDebutAffectation: control.get('dateDebutAffectation')?.value || this.getTodayDate(),
                    dateDebut: control.get('dateDebutAffectation')?.value || this.getTodayDate(),
                    nature: control.get('nature')?.value || 'PERMANENTE'
                };
            })
            .filter((affectation): affectation is UpdateAffectationRequest => affectation !== null);
    }

    private patchEditForm(personnel: PersonnelDetailsDto): void {
        this.currentStep.set(1);
        this.initializeEditAffectations(personnel);
        this.form.get('email')?.enable({ emitEvent: false });

     

        this.form.patchValue({
            matricule: personnel.matricule,
            nom: personnel.nom,
            prenom: personnel.prenom,
            dateRecrutement: this.toDateInputValue(personnel.dateRecrutement),
            dateNaissance: this.toDateInputValue(personnel.dateNaissance),
            email: personnel.email,
            statut: personnel.statut ?? '',
            grade: personnel.grade ?? '',
            entiteId: this.organisationService.findNodeById(this.organizationTree(), personnel.entiteId),
            createUser: false,
            userRole: ''
        }, { emitEvent: false });
        this.form.get('email')?.setValue(personnel.email, { emitEvent: false });
        this.form.get('email')?.disable({ emitEvent: false });
        this.form.get('createUser')?.disable({ emitEvent: false });
        this.form.get('userRole')?.disable({ emitEvent: false });
        this.form.markAsPristine();
        this.form.markAsUntouched();
        this.affectationsFormArray.markAsPristine();
        this.affectationsFormArray.markAsUntouched();
        this.formStatusChanged.update((value) => value + 1);
    }

    private resetCreateForm(): void {
        this.currentStep.set(1);
        this.form.get('email')?.enable({ emitEvent: false });
        this.form.get('createUser')?.enable({ emitEvent: false });
        this.toggleUserRoleControl(false, false);

        this.clearAffectations();
        this.affectationsFormArray.push(this.createAffectationGroup());

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
            userRole: ''
        }, { emitEvent: false });

        this.form.markAsPristine();
        this.form.markAsUntouched();
        this.affectationsFormArray.markAsPristine();
        this.affectationsFormArray.markAsUntouched();
        this.formStatusChanged.update((value) => value + 1);
    }

    private initializeEditAffectations(personnel: PersonnelDetailsDto): void {
        const affectationGroups = personnel.affectations.map((affectation) => {
            const entiteNode = this.organisationService.findNodeById(
                this.organizationTree(),
                affectation.entiteId
            );
            const fonctionNode = this.findFonctionNodeById(
                this.fonctionsTree(),
                affectation.fonctionId
            );

            return this.createAffectationGroup({
                id: affectation.id,
                entiteId: entiteNode,
                fonctionId: fonctionNode,
                dateDebutAffectation: this.toDateInputValue(this.getAffectationStartDate(affectation)),
                nature: affectation.nature
            });
        });

        const affectationsArray = this.fb.array(
            affectationGroups.length > 0
                ? affectationGroups
                : [this.createAffectationGroup()]
        );

        this.form.setControl(
            'affectations',
            affectationsArray as unknown as FormArray
        );
    }

    private clearAffectations(): void {
        while (this.affectationsFormArray.length > 0) {
            this.affectationsFormArray.removeAt(0);
        }
    }

    private createAffectationGroup(value?: {
        id?: number;
        entiteId?: OrganizationTreeNode | null;
        fonctionId?: PrimeNgTreeNode | null;
        dateDebutAffectation?: string;
        nature?: string;
    }) {
        return this.fb.group({
            id: [value?.id ?? 0],
            entiteId: [value?.entiteId ?? null as OrganizationTreeNode | null, Validators.required],
            fonctionId: [value?.fonctionId ?? null as PrimeNgTreeNode | null, Validators.required],
            dateDebutAffectation: [value?.dateDebutAffectation ?? this.getTodayDate(), Validators.required],
            nature: [value?.nature ?? 'PERMANENTE']
        });
    }

    private getAffectationStartDate(affectation: PersonnelEditAffectationDto): string | null | undefined {
        return affectation.dateDebutAffectation ?? affectation.dateDebut;
    }

    private toggleUserRoleControl(isEnabled: boolean, emitEvent = true): void {
        const userRoleControl = this.form.get('userRole');

        if (!userRoleControl) {
            return;
        }

        if (isEnabled) {
            userRoleControl.enable({ emitEvent });
        } else {
            userRoleControl.reset('', { emitEvent });
            userRoleControl.disable({ emitEvent });
        }
    }

    private findFonctionNodeById(nodes: PrimeNgTreeNode[], id: number | null): PrimeNgTreeNode | null {
        if (!id) {
            return null;
        }

        for (const node of nodes) {
            const nodeId = this.extractNodeId(node);
            if (nodeId === id) {
                return node;
            }

            if (node.children?.length) {
                const found = this.findFonctionNodeById(node.children, id);
                if (found) {
                    return found;
                }
            }
        }

        return null;
    }

    private extractNodeId(node: unknown): number | null {
        if (node == null) {
            return null;
        }

        if (typeof node === 'number') {
            return Number.isNaN(node) ? null : node;
        }

        if (typeof node === 'string') {
            return this.parseIdValue(node);
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
            return this.parseIdValue(treeNode.key);
        }

        if (treeNode.id != null) {
            return this.parseIdValue(treeNode.id);
        }

        if (typeof treeNode.data === 'number') {
            return Number.isNaN(treeNode.data) ? null : treeNode.data;
        }

        if (treeNode.data && typeof treeNode.data === 'object') {
            const dataNode = treeNode.data as { id?: string | number | null };

            if (dataNode.id != null) {
                return this.parseIdValue(dataNode.id);
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

    private getTodayDate(): string {
        return new Date().toISOString().split('T')[0];
    }

    private toDateInputValue(value: string | null | undefined): string {
        if (!value) {
            return '';
        }

        return value.split('T')[0];
    }
}
