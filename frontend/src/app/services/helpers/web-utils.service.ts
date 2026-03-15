import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class WebUtilsService {
    /**
     * Prépare les paramètres pour un GET simple
     */
    toHttpParams(filters: any): HttpParams {
        const cleanFilters: any = {};

        Object.keys(filters).forEach(key => {
            const value = filters[key];
            // On ne garde que ce qui a une valeur réelle (évite ?parentId=null)
            if (value !== null && value !== undefined && value !== '') {
                cleanFilters[key] = value.toString();
            }
        });

        return new HttpParams({ fromObject: cleanFilters });
    }
}