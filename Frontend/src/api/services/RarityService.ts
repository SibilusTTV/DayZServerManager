/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { RarityFile } from '../models/RarityFile';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class RarityService {
    /**
     * @param instanceId
     * @param name
     * @returns RarityFile OK
     * @throws ApiError
     */
    public static getApiRarityGet(
        instanceId?: number,
        name?: string,
    ): CancelablePromise<RarityFile> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Rarity/Get',
            query: {
                'instanceId': instanceId,
                'name': name,
            },
        });
    }
    /**
     * @param instanceId
     * @param name
     * @param requestBody
     * @returns any OK
     * @throws ApiError
     */
    public static putApiRarityUpdate(
        instanceId?: number,
        name?: string,
        requestBody?: RarityFile,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/Rarity/Update',
            query: {
                'instanceId': instanceId,
                'name': name,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
