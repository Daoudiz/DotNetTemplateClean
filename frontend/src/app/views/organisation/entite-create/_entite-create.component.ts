import { Component, EventEmitter, Input, OnInit, Output, inject, signal, DestroyRef} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TreeSelectModule } from 'primeng/treeselect';
import { OrganizationService } from '../../../services/organisation/organisation.service';
import { NotificationService } from '../../../services/notification.service';
import { ValidationService } from '../../../services/user/validation.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EntiteItemSearchResponse, EntiteSaveDto, OrganizationTreeNode, TypeEntite } from '../../../models/organisation/organisation-model';
import { forkJoin, of, Subject, switchMap, tap } from 'rxjs';

@Component({
  selector: 'app-entite-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TreeSelectModule],
  templateUrl: './entite-create.component.html'
})
export class EntiteCreateComponent implements OnInit {

//#region --- INJECTIONS & PROPRIÉTÉS ---
  private readonly fb = inject(FormBuilder);
  private readonly organisationService = inject(OrganizationService);
  private readonly notification = inject(NotificationService);
  public readonly valService = inject(ValidationService);
  private readonly debug = true;
  private readonly destroyRef = inject(DestroyRef);
  private readonly entiteId$ = new Subject<number | null>();

  private log(...args: any[]): void {
    if (!this.debug) return;
    // eslint-disable-next-line no-console
    console.log('[EntiteCreateComponent]', ...args);
  }

  public form!: FormGroup;
  public isEditMode = false;
  readonly loading = signal<boolean>(false);
  readonly organizationTree = signal<OrganizationTreeNode[]>([]);
  private _entiteId: number | null = null;

  @Input() types: TypeEntite[] = [];

  @Output() cancel = new EventEmitter<void>();
  @Output() entiteSaved = new EventEmitter<void>();

  constructor() {

    // ON CRÉE LE FORMULAIRE ICI
    this.form = this.fb.nonNullable.group({
      code: ['', Validators.required],
      libelle: ['', Validators.required],
      typeEntiteId: [null as number | null, Validators.required],
      rattachementEntiteId: [null as OrganizationTreeNode | null]
    });

    // On configure le tuyau une seule fois
    this.entiteId$.pipe(
      tap(() => this.loading.set(true)),
      // switchMap annule la requête précédente si l'ID change entre temps
      switchMap(id =>
        // forkJoin lance les deux appels en parallèle et attend que les DEUX soient finis
        forkJoin({
          nodes: this.organisationService.getOrganizationTree(),
          entite: id ? this.organisationService.getEntiteById(id) : of(null)
        })
      ),
      takeUntilDestroyed() // Auto-unsubscribe (Angular 16+)
    ).subscribe(({ nodes, entite }) => {

      //On met TOUJOURS à jour l'arbre, qu'on soit en édition ou création
      const treeWithKeys = this.organisationService.addKeysToTree(nodes);
      this.organizationTree.set(treeWithKeys);
      
      // On utilise queueMicrotask pour laisser à Angular le temps de
      // propager les options de l'arbre au composant p-treeSelect
      queueMicrotask(() => {
        if (entite) {
          // On s'assure que _entiteId est bien synchronisé avec l'objet reçu
          this._entiteId = entite.id?? null;
          this.patchForm(entite);
        } else {
          this._entiteId = null; // On repasse en mode création
          this.form.reset();
        }
        this.loading.set(false);
      });
    });
  }

  //#endregion

//#region --- GESTION DE L'INPUT D'UNITÉ À ÉDITER ---

  @Input() set entiteToEdit(value: EntiteItemSearchResponse | null) {
    const id = value?.id ?? null;
    this._entiteId = id;
    this.isEditMode = !!id;

    // On pousse l'ID dans le tuyau, switchMap s'occupe du reste
    this.entiteId$.next(id);
  }

 

  private patchForm(dto: EntiteSaveDto): void {
    
    // On cherche le nœud correspondant dans l'arbre aplati (ou via une fonction récursive)
    const selectedNode = this.organisationService.findNodeById(this.organizationTree(), dto.rattachementEntiteId ?? null );
    this.form.patchValue({
      code: dto.code,
      libelle: dto.libelle,
      typeEntiteId: dto.typeEntiteId,     
      rattachementEntiteId: selectedNode
      
    });
  }
  
  //#endregion
  
//#region --- INITIALISATION ---
  ngOnInit(): void {

  }

  //#endregion

//#region --- ACTIONS PRINCIPALES (Submit, Cancel) ---
  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.value;
    // PrimeNG TreeSelect peut stocker un TreeNode complet, pas uniquement l'id.
    const rawRattachement = raw.rattachementEntiteId; 

    // Extraction propre de l'ID du TreeSelect (Objet ou ID simple)
    const rattachementId = rawRattachement?.key
      ? Number(rawRattachement.key)
      : (rawRattachement ? Number(rawRattachement) : null);

    const payload: EntiteSaveDto = {
      ...raw,
      rattachementEntiteId: Number.isFinite(rattachementId as number) ? (rattachementId as number) : undefined,
      id: this._entiteId ?? undefined
    };

    this.loading.set(true);

    const request$ = this._entiteId
      ? this.organisationService.updateEntite(this._entiteId, payload)
      : this.organisationService.createEntite(payload);

    request$.pipe(
      tap(() => {
        this.notification.success(this._entiteId ? 'Mise à jour réussie' : 'Création réussie');
        this.entiteSaved.emit();
        this.onCancel();
      }),
      
      tap(() => this.loading.set(false))
    ).subscribe();    
  }

  onCancel(): void {
    this.form.reset();
    this.cancel.emit();
  }

  //#endregion  
}

