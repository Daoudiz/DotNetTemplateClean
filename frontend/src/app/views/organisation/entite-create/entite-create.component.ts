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
    ReactiveFormsModule
} from '@angular/forms';
import { TreeSelectModule } from 'primeng/treeselect';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
    forkJoin,
    of,    
    finalize
} from 'rxjs';

import { OrganizationService } from '../../../services/organisation/organisation.service';
import { NotificationService } from '../../../services/notification.service';
import { ValidationService } from '../../../services/user/validation.service';

import {
    EntiteItemSearchResponse,
    EntiteSaveDto,
    OrganizationTreeNode,
    TypeEntite
} from '../../../models/organisation/organisation-model';

@Component({
    selector: 'app-entite-create',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, TreeSelectModule],
    templateUrl: './entite-create.component.html'
})
export class EntiteCreateComponent {

//#region ─────────────────── INJECTIONS & ETATS ───────────────────
    private readonly fb = inject(FormBuilder);
    private readonly organisationService = inject(OrganizationService);
    private readonly notification = inject(NotificationService);
    public readonly valService = inject(ValidationService);
    private readonly destroyRef = inject(DestroyRef);


    private readonly entiteId = signal<number | null>(null);
    readonly isEditMode = computed(() => this.entiteId() !== null);
    readonly loading = signal(false);
    readonly organizationTree = signal<OrganizationTreeNode[]>([]);

    @Input() types: TypeEntite[] = [];

    @Output() cancel = new EventEmitter<void>();
    @Output() entiteSaved = new EventEmitter<void>();

    // ───────────────────────── FORMULAIRE TYPÉ
    readonly form = this.fb.nonNullable.group({
        code: ['', Validators.required],
        libelle: ['', Validators.required],
        typeEntiteId: [null as number | null, Validators.required],
        rattachementEntiteId: [null as OrganizationTreeNode | null]
    });

//#endregion

//#region  ───────────────────────── INPUT EXTERNE ──────
    @Input()
    set entiteToEdit(value: EntiteItemSearchResponse | null) {
       
        // protection anti-clic rapide
        if (this.loading()) return;       
        this.entiteId.set(value?.id ?? null);        
    }

    constructor() {
        this.setupDataLoadingEffect();
    }

    // ───────────────────────── CHARGEMENT RÉACTIF
    private setupDataLoadingEffect(): void {
        effect(() => {
            const id = this.entiteId();
            this.loadData(id);
        });
    }

    private loadData(id: number | null): void {
        this.loading.set(true);

        forkJoin({
            nodes: this.organisationService.getOrganizationTree(),
            entite: id
                ? this.organisationService.getEntiteById(id)
                : of(null)
        })
            .pipe(
                takeUntilDestroyed(this.destroyRef),
                
                finalize(() => this.loading.set(false))
            )
            .subscribe(({ nodes, entite }) => {
                const tree = this.organisationService.addKeysToTree(nodes);
                this.organizationTree.set(tree);
                // On s'assure que l'arbre est "digéré" par le composant UI
                queueMicrotask(() => {
                    if (entite) {
                        this.patchForm(entite);
                    } else {
                        this.form.reset();
                    }
                });
            });
    }

    // ───────────────────────── PATCH FORMULAIRE
    private patchForm(dto: EntiteSaveDto): void {
        const selectedNode = this.organisationService.findNodeById(
            this.organizationTree(),
            dto.rattachementEntiteId ?? null
        );

        this.form.patchValue({
            code: dto.code,
            libelle: dto.libelle,
            typeEntiteId: dto.typeEntiteId,
            rattachementEntiteId: selectedNode
        });
    }
//#endregion

//#region ────────────────────ACTIONS PRINCIPALES ────────────────────────
    onSubmit(): void {
        if (this.loading()) return; // protection double clic

        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        const raw = this.form.getRawValue();
        const { rattachementEntiteId, ...rest } = raw;

        const rattachementId =
            rattachementEntiteId?.key
                ? Number(rattachementEntiteId.key)
                : null;

        const typeEntiteId = raw.typeEntiteId ?? undefined; // transforme null en undefined

        const payload: EntiteSaveDto = {
            ...rest,
            typeEntiteId: typeEntiteId as number, // assure TS que c'est bien un number
            rattachementEntiteId: rattachementId ?? undefined,
            id: this.entiteId() ?? undefined
        };
        this.loading.set(true);

        const request$ = this.isEditMode()
            ? this.organisationService.updateEntite(this.entiteId()!, payload)
            : this.organisationService.createEntite(payload);

        request$
            .pipe(
                takeUntilDestroyed(this.destroyRef),
                
                finalize(() => this.loading.set(false))
            )
            .subscribe(() => {
                this.notification.success(
                    this.isEditMode()
                        ? 'Mise à jour réussie'
                        : 'Création réussie'
                );
                this.entiteSaved.emit();
                this.onCancel();
            });
    }


    onCancel(): void {
        this.form.reset();
        this.cancel.emit();
    }

//#endregion
}