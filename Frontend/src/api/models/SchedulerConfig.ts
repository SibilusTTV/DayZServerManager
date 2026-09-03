/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CustomMessage } from './CustomMessage';
export type SchedulerConfig = {
    id?: string | null;
    instanceId?: number;
    useNickFilter?: boolean;
    filteredNickMsg?: string | null;
    badNames?: Array<string> | null;
    timeout?: number;
    restartOnUpdate?: boolean;
    restartInterval?: number;
    customMessages?: Array<CustomMessage> | null;
};

