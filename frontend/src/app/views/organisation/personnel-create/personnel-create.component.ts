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
    ValidationErrors,
    Validators
} from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable, finalize, forkJoin, of } from 'rxjs';
import { ButtonModule, ModalModule } from '@coreui/angular';
import { IconModule } from '@coreui/icons-angular'; 

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
    NatureFonction,
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
    imports: [
        CommonModule, ReactiveFormsModule, TreeSelectModule, 
        TabsModule, ModalModule, ButtonModule, IconModule],
    templateUrl: './personnel-create.component.html',
    styleUrls: ['./personnel-create.component.scss']
})
export class PersonnelCreateComponent {
    private static readonly overlappingAffectationsErrorKey = 'overlappingAffectations';
    private static readonly missingInitialEntiteAffectationErrorKey = 'missingInitialEntiteAffectation';

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
    readonly natureFonctions = signal<NatureFonction[]>([]);
    readonly statuts = signal<StatutPersonnel[]>([]);
    readonly roles = signal<any[]>([]);
    private readonly defaultNature = 'Titulaire';
    private readonly overlappingAffectationsMessage =
        'Un personnel ne peut pas avoir deux affectations avec la meme entite et la meme fonction sur des periodes qui se chevauchent.';
    private readonly missingInitialEntiteAffectationMessage =
        "Au moins une affectation doit correspondre a l'entite initiale du personnel.";
    readonly isEndAffectationDialogOpen = signal(false);
    readonly selectedAffectationIndex = signal<number | null>(null);
    readonly endAffectationDate = signal('');

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
        affectations: this.createAffectationsFormArray()
    }, {
        validators: [(control: AbstractControl) => this.validateInitialEntiteAffectation(control)]
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
        return this.affectationsFormArray.length > 0
            && this.affectationsFormArray.valid
            && !this.hasMissingInitialEntiteAffectationError();
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
            natureFonctions: this.personnelService.getNatureFonctions(),
            statuts: this.personnelService.getStatutPersonnel(),
            roles: this.userService.getRoles(),
            personnel: id ? this.personnelService.getPersonnelById(id) : of(null)
        })
            .pipe(
                takeUntilDestroyed(this.destroyRef),
                finalize(() => this.loading.set(false))
            )
            .subscribe(({ organizationTree, fonctionsTree, natureFonctions, statuts, roles, personnel }) => {
                this.organizationTree.set(this.organisationService.addKeysToTree(organizationTree));
                this.fonctionsTree.set(fonctionsTree);
                this.natureFonctions.set(natureFonctions);
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
        this.refreshAffectationsValidation();
    }

    removeAffectation(index: number): void {
        if (this.affectationsFormArray.length > 1) {
            this.affectationsFormArray.removeAt(index);
            this.refreshAffectationsValidation();
            this.notification.info('Affectation supprimee');
        } else {
            this.notification.warn('Au moins une affectation est requise');
        }
    }

    onAffectationAction(index: number): void {
        if (this.isExistingAffectation(index)) {
            this.openEndAffectationDialog(index);
            return;
        }

        this.removeAffectation(index);
    }

    isExistingAffectation(index: number): boolean {
        if (!this.isEditMode()) {
            return false;
        }

        const idValue = Number(this.affectationsFormArray.at(index).get('id')?.value || 0);
        return idValue > 0;
    }

    isAffectationReadOnly(index: number): boolean {
        return this.isEditMode() && this.isExistingAffectation(index);
    }

    getAffectationActionTitle(index: number): string {
        if (this.isExistingAffectation(index)) {
            return 'Cloturer cette affectation';
        }

        if (this.affectationsFormArray.length === 1) {
            return 'Au moins une affectation est requise';
        }

        return 'Supprimer cette affectation';
    }

    closeEndAffectationDialog(): void {
        this.isEndAffectationDialogOpen.set(false);
        this.selectedAffectationIndex.set(null);
        this.endAffectationDate.set('');
    }

    confirmEndAffectation(): void {
        const index = this.selectedAffectationIndex();
        const endDate = this.endAffectationDate();

        if (index == null || !endDate) {
            this.notification.warn('Date de fin affectation requise');
            return;
        }

        const affectation = this.affectationsFormArray.at(index);
        const startDate = affectation.get('dateDebutAffectation')?.value as string | null;

        if (startDate && endDate < startDate) {
            this.notification.warn('La date de fin doit etre superieure ou egale a la date de debut');
            return;
        }

        affectation.patchValue({ dateFinAffectation: endDate });
        this.refreshAffectationsValidation();
        this.closeEndAffectationDialog();
    }

    getAffectationControl(index: number, controlName: string): AbstractControl | null {
        return this.affectationsFormArray.at(index).get(controlName);
    }

    hasOverlappingAffectationsError(): boolean {
        return this.affectationsFormArray.hasError(PersonnelCreateComponent.overlappingAffectationsErrorKey);
    }

    hasMissingInitialEntiteAffectationError(): boolean {
        return this.form.hasError(PersonnelCreateComponent.missingInitialEntiteAffectationErrorKey);
    }

    getOverlappingAffectationsErrorMessage(): string {
        const error = this.affectationsFormArray.getError(PersonnelCreateComponent.overlappingAffectationsErrorKey) as
            | { message?: string }
            | undefined;

        return error?.message ?? this.overlappingAffectationsMessage;
    }

    getMissingInitialEntiteAffectationErrorMessage(): string {
        const error = this.form.getError(PersonnelCreateComponent.missingInitialEntiteAffectationErrorKey) as
            | { message?: string }
            | undefined;

        return error?.message ?? this.missingInitialEntiteAffectationMessage;
    }

    isOverlappingAffectation(index: number): boolean {
        const error = this.affectationsFormArray.getError(PersonnelCreateComponent.overlappingAffectationsErrorKey) as
            | { overlappingIndices?: number[] }
            | undefined;

        return error?.overlappingIndices?.includes(index) ?? false;
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

    getNatureLabel(value: string | null | undefined): string {
        if (!value) {
            return '-';
        }

        return this.natureFonctions().find((nature) => nature.value === value)?.displayName ?? value;
    }

    onSubmit(): void {
        if (this.loading()) {
            return;
        }

        if (this.form.invalid || this.affectationsFormArray.length === 0) {
            this.form.markAllAsTouched();
            this.affectationsFormArray.markAllAsTouched();
            this.notification.error(
                this.hasOverlappingAffectationsError()
                    ? this.getOverlappingAffectationsErrorMessage()
                    : this.hasMissingInitialEntiteAffectationError()
                        ? this.getMissingInitialEntiteAffectationErrorMessage()
                    : 'Veuillez remplir tous les champs requis'
            );
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
                    nature: control.get('nature')?.value || this.defaultNature
                };
            })
            .filter((affectation): affectation is CreateAffectationRequest => affectation !== null);
    }

    private buildUpdateAffectationsPayload(): UpdateAffectationRequest[] {
        return this.affectationsFormArray.controls
            .map((control): UpdateAffectationRequest | null => {
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
                    nature: control.get('nature')?.value || this.defaultNature,
                    dateFinAffectation: control.get('dateFinAffectation')?.value || null
                };
            })
            .filter((affectation): affectation is NonNullable<typeof affectation> => affectation !== null);
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
        const affectationsArray = this.createAffectationsFormArray();
        this.form.setControl('affectations', affectationsArray as unknown as FormArray);

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
                nature: affectation.nature || this.defaultNature,
                dateFinAffectation: this.toDateInputValue(affectation.dateFinAffectation)
            });
        });

        if (affectationGroups.length === 0) {
            this.affectationsFormArray.push(this.createAffectationGroup());
        } else {
            affectationGroups.forEach((group) => {
                this.affectationsFormArray.push(group);
            });
        }

        this.applyEditModeReadOnlyState();
        this.refreshAffectationsValidation(false);
    }

    private clearAffectations(): void {
        while (this.affectationsFormArray.length > 0) {
            this.affectationsFormArray.removeAt(0);
        }
    }

    private createAffectationsFormArray(): FormArray {
        return this.fb.array([], {
            validators: [(control: AbstractControl) => this.validateOverlappingAffectations(control)]
        });
    }

    private createAffectationGroup(value?: {
        id?: number;
        entiteId?: OrganizationTreeNode | null;
        fonctionId?: PrimeNgTreeNode | null;
        dateDebutAffectation?: string;
        nature?: string;
        dateFinAffectation?: string;
    }) {
        return this.fb.group({
            id: [value?.id ?? 0],
            entiteId: [value?.entiteId ?? null as OrganizationTreeNode | null, Validators.required],
            fonctionId: [value?.fonctionId ?? null as PrimeNgTreeNode | null, Validators.required],
            dateDebutAffectation: [value?.dateDebutAffectation ?? this.getTodayDate(), Validators.required],
            nature: [value?.nature ?? this.defaultNature, Validators.required],
            dateFinAffectation: [value?.dateFinAffectation ?? '']
        });
    }

    private validateOverlappingAffectations(control: AbstractControl): ValidationErrors | null {
        if (!(control instanceof FormArray)) {
            return null;
        }

        const overlapIndices = new Set<number>();
        const groupedAffectations = new Map<
            string,
            Array<{ index: number; startDate: number; endDate: number }>
        >();

        control.controls.forEach((group, rowIndex) => {
            const entiteId = this.extractNodeId(group.get('entiteId')?.value);
            const fonctionId = this.extractNodeId(group.get('fonctionId')?.value);
            const startDateValue = group.get('dateDebutAffectation')?.value as string | null | undefined;
            const endDateValue = group.get('dateFinAffectation')?.value as string | null | undefined;
            const startDate = this.parseDateInput(startDateValue);
            const endDate = this.parseDateInput(endDateValue) ?? Number.POSITIVE_INFINITY;

            if (!entiteId || !fonctionId || startDate === null) {
                return;
            }

            const pairKey = `${entiteId}:${fonctionId}`;
            const rowsForPair = groupedAffectations.get(pairKey) ?? [];

            for (const existingRow of rowsForPair) {
                if (this.areRangesOverlapping(startDate, endDate, existingRow.startDate, existingRow.endDate)) {
                    overlapIndices.add(existingRow.index);
                    overlapIndices.add(rowIndex);
                }
            }

            rowsForPair.push({
                index: rowIndex,
                startDate,
                endDate
            });
            groupedAffectations.set(pairKey, rowsForPair);
        });

        if (overlapIndices.size === 0) {
            return null;
        }

        return {
            [PersonnelCreateComponent.overlappingAffectationsErrorKey]: {
                overlappingIndices: Array.from(overlapIndices).sort((left, right) => left - right),
                message: this.overlappingAffectationsMessage
            }
        };
    }

    private validateInitialEntiteAffectation(control: AbstractControl): ValidationErrors | null {
        const initialEntiteId = this.extractNodeId(control.get('entiteId')?.value);
        const affectationsControl = control.get('affectations');

        if (initialEntiteId == null || !(affectationsControl instanceof FormArray)) {
            return null;
        }

        const hasAffectationInInitialEntite = affectationsControl.controls.some((group) => {
            const affectationEntiteId = this.extractNodeId(group.get('entiteId')?.value);
            return affectationEntiteId === initialEntiteId;
        });

        if (hasAffectationInInitialEntite) {
            return null;
        }

        return {
            [PersonnelCreateComponent.missingInitialEntiteAffectationErrorKey]: {
                message: this.missingInitialEntiteAffectationMessage
            }
        };
    }

    private areRangesOverlapping(
        firstStart: number,
        firstEnd: number,
        secondStart: number,
        secondEnd: number
    ): boolean {
        return firstStart <= secondEnd && secondStart <= firstEnd;
    }

    private parseDateInput(value: string | null | undefined): number | null {
        if (!value) {
            return null;
        }

        const parsedValue = Date.parse(`${value}T00:00:00Z`);
        return Number.isNaN(parsedValue) ? null : parsedValue;
    }

    private applyEditModeReadOnlyState(): void {
        if (!this.isEditMode()) {
            return;
        }

        this.affectationsFormArray.controls.forEach((group) => {
            const idValue = Number(group.get('id')?.value || 0);
            if (idValue <= 0) {
                return;
            }

            group.get('entiteId')?.disable({ emitEvent: false });
            group.get('fonctionId')?.disable({ emitEvent: false });
            group.get('dateDebutAffectation')?.disable({ emitEvent: false });
            group.get('nature')?.disable({ emitEvent: false });
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

    private refreshAffectationsValidation(emitEvent = true): void {
        this.affectationsFormArray.updateValueAndValidity({ emitEvent });
        this.formStatusChanged.update((value) => value + 1);
    }

    private getTodayDate(): string {
        return new Date().toISOString().split('T')[0];
    }

    private openEndAffectationDialog(index: number): void {
        const affectation = this.affectationsFormArray.at(index);
        const currentEndDate = affectation.get('dateFinAffectation')?.value as string | null;

        this.selectedAffectationIndex.set(index);
        this.endAffectationDate.set(currentEndDate || this.getTodayDate());
        this.isEndAffectationDialogOpen.set(true);
    }

    private toDateInputValue(value: string | null | undefined): string {
        if (!value) {
            return '';
        }

        return value.split('T')[0];
    }
}
