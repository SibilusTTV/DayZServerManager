/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CustomMessage } from './CustomMessage';
import type { InstanceClientMod } from './InstanceClientMod';
import type { InstanceServerMod } from './InstanceServerMod';
export type Instance = {
    id?: number;
    serverFolder?: string | null;
    hostName?: string | null;
    missionName?: string | null;
    vanillaMissionName?: string | null;
    missionTemplateName?: string | null;
    serverConfigName?: string | null;
    profileName?: string | null;
    steamPort?: number;
    serverPort?: number;
    steamQueryPort?: number;
    rConPort?: number;
    rConPassword?: string | null;
    cpuCount?: number;
    noFilePatching?: boolean;
    doLogs?: boolean;
    adminLog?: boolean;
    freezeCheck?: boolean;
    netLog?: boolean;
    limitFPS?: number;
    mapName?: string | null;
    restartOnUpdate?: boolean;
    restartInterval?: number;
    autoStartServer?: boolean;
    makeBackups?: boolean;
    deleteBackups?: boolean;
    backupPath?: string | null;
    maxKeepTime?: number;
    clientMods?: Array<InstanceClientMod> | null;
    serverMods?: Array<InstanceServerMod> | null;
    customMessages?: Array<CustomMessage> | null;
};

