/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { HttpStatusCode } from '../models/HttpStatusCode';
import type { Mod } from '../models/Mod';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class ModService {
    /**
     * @param id
     * @returns Mod OK
     * @throws ApiError
     */
    public static getApiModGet(
        id?: string,
    ): CancelablePromise<Mod> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Mod/Get',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @returns Mod OK
     * @throws ApiError
     */
    public static getApiModGetMods(): CancelablePromise<Array<Mod>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Mod/GetMods',
        });
    }
    /**
     * @param id
     * @returns HttpStatusCode OK
     * @throws ApiError
     */
    public static deleteApiModDeleteMod(
        id?: string,
    ): CancelablePromise<HttpStatusCode> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/Mod/DeleteMod',
            query: {
                'id': id,
            },
        });
    }
}
