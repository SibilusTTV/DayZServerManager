/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { Player } from '../models/Player';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class PlayerService {
    /**
     * @returns Player OK
     * @throws ApiError
     */
    public static getApiPlayerGetPlayers(): CancelablePromise<Array<Player>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Player/GetPlayers',
        });
    }
}
