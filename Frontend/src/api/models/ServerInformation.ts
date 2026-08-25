/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ConnectedPlayer } from './ConnectedPlayer';
export type ServerInformation = {
    managerStatus?: string | null;
    dayzServerStatus?: string | null;
    playersCount?: number;
    players?: Array<ConnectedPlayer> | null;
    chatLog?: string | null;
    adminLog?: string | null;
};

