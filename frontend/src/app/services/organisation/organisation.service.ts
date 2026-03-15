import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EntiteItemSearchResponse, EntiteSaveDto, EntiteSearchRequest, OrganizationTreeNode, TypeEntite } from '../../models/organisation/organisation-model';
import { WebUtilsService } from '../helpers/web-utils.service';
import { PagedResult } from '../../models/generic/generics';

@Injectable({ providedIn: 'root' })
export class OrganizationService {

    private http = inject(HttpClient);
    private webUtils = inject(WebUtilsService);
    private readonly apiUrl = `${environment.apiUrl}`;;

    searchEntites(filters: EntiteSearchRequest): Observable<PagedResult<EntiteItemSearchResponse>> {
        const params = this.webUtils.toHttpParams(filters);
        return this.http.get<PagedResult<EntiteItemSearchResponse>>(`${this.apiUrl}/entite/EntiteSearch`, { params });
    }

    getAllTypes(): Observable<TypeEntite[]> {
        return this.http.get<TypeEntite[]>(`${this.apiUrl}/entite/TypesEntites`);
    }

    getOrganizationTree(): Observable<OrganizationTreeNode[]> {
        return this.http.get<OrganizationTreeNode[]>(`${this.apiUrl}/entite/OrganizationTree`);
    }

    getEntiteById(id: number): Observable<EntiteSaveDto> {
        return this.http.get<EntiteSaveDto>(`${this.apiUrl}/entite/${id}`);
    }

    createEntite(dto: EntiteSaveDto): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/entite/CreateEntite`, dto);
    }

    updateEntite(id: number, dto: EntiteSaveDto): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/entite/${id}`, dto);
    }

    deleteEntite(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/entite/${id}`);
    }

    /**
    * Transforme récursivement l'arborescence brute en une structure compatible PrimeNG.
    * * Cette méthode assure que chaque nœud possède une propriété 'key' unique, indispensable 
    * au TreeSelect pour la sélection et l'affichage. Elle utilise une stratégie de repli (fallback)
    * pour générer la clé : la clé existante, sinon l'identifiant dans 'data', sinon le libellé.
    * * @param nodes - Liste de nœuds provenant de l'API (souvent dépourvus de clés 'key').
    * @returns Un nouvel arbre où chaque nœud et ses descendants possèdent une clé définie.
    */
    public addKeysToTree(nodes: OrganizationTreeNode[]): OrganizationTreeNode[] {
        return nodes.map(node => ({
            ...node,
            key: node.key ?? (node.data != null ? String(node.data) : node.label),
            //expanded: !!(node.children && node.children.length),
            children: node.children ? this.addKeysToTree(node.children) : undefined
        }));
    }

    /**
    * Recherche récursivement un nœud dans l'arborescence à partir de son identifiant.
    * * Cette méthode est cruciale pour le composant p-treeSelect de PrimeNG car elle permet 
    * de transformer un ID numérique (venant du backend) en un objet TreeNode complet,
    * garantissant ainsi l'affichage correct du libellé dans le champ de saisie.
    * * @param nodes - Le tableau de nœuds (l'arbre) dans lequel effectuer la recherche.
    * @param id - L'identifiant numérique de l'entité à retrouver.
    * @returns Le nœud correspondant (OrganizationTreeNode) ou null si l'ID n'est pas trouvé.
    */
    public findNodeById(nodes: OrganizationTreeNode[], id: number | null): OrganizationTreeNode | null {
        if (!id || !nodes) return null;
        const idStr = id.toString();

        for (const node of nodes) {
            if (node.key === idStr) return node;
            if (node.children) {
                const found = this.findNodeById(node.children, id);
                if (found) return found;
            }
        }
        return null;
    }
}
