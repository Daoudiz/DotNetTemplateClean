import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';

type QueryParamPrimitive = string | number | boolean | Date;
type QueryParamValue = QueryParamPrimitive | null | undefined;
type QueryParamCollection = QueryParamValue | ReadonlyArray<QueryParamValue>;

@Injectable({ providedIn: 'root' })
export class WebUtilsService {
    /**
     * Builds HTTP query params from a typed filter object.
     */
    toHttpParams<T extends object>(filters: T): HttpParams {
        const cleanFilters: Record<string, string | ReadonlyArray<string>> = {};

        Object.entries(filters as Record<string, unknown>).forEach(([key, rawValue]) => {
            if (Array.isArray(rawValue)) {
                const formattedValues = rawValue
                    .filter((item): item is QueryParamPrimitive => item !== null && item !== undefined && item !== '')
                    .map((item) => this.toQueryParamString(item));

                if (formattedValues.length > 0) {
                    cleanFilters[key] = formattedValues;
                }
                return;
            }

            const value = rawValue as QueryParamValue;
            if (value !== null && value !== undefined && value !== '') {
                cleanFilters[key] = this.toQueryParamString(value);
            }
        });

        return new HttpParams({ fromObject: cleanFilters });
    }

    private toQueryParamString(value: QueryParamPrimitive): string {
        return value instanceof Date ? value.toISOString() : String(value);
    }
}
