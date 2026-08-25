/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { Player } from '../models/Player';
import type { ServerPlayerInformation } from '../models/ServerPlayerInformation';
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
    /**
     * @param id
     * @returns Player OK
     * @throws ApiError
     */
    public static getApiPlayerGetPlayer(
        id?: string,
    ): CancelablePromise<Player> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Player/GetPlayer',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @param id
     * @returns ServerPlayerInformation OK
     * @throws ApiError
     */
    public static getApiPlayerGetServerPlayerInformation(
        id?: string,
    ): CancelablePromise<Array<ServerPlayerInformation>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Player/GetServerPlayerInformation',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @param playerId
     * @param instanceId
     * @param isWhitelisted
     * @param isBanned
     * @returns any OK
     * @throws ApiError
     */
    public static postApiPlayerCreateServerPlayer(
        playerId?: string,
        instanceId?: string,
        isWhitelisted?: boolean,
        isBanned?: boolean,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Player/CreateServerPlayer',
            query: {
                'playerId': playerId,
                'instanceId': instanceId,
                'isWhitelisted': isWhitelisted,
                'isBanned': isBanned,
            },
        });
    }
}
