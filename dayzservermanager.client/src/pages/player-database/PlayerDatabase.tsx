

import * as React from "react";
import KickButton from "../../common/components/kick-button/KickButton";
import UnbanButton from "../../common/components/unban-button/UnbanButton";
import BanButton from "../../common/components/ban-button/BanButton";
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import UnpublishedIcon from '@mui/icons-material/Unpublished';
import { ColumnActionsMode, ContextualMenu, DirectionalHint, IColumn, IContextualMenuItem, IContextualMenuProps, initializeIcons, SelectionMode, ShimmeredDetailsList, TextField } from "@fluentui/react";
import ReloadButton from "../../common/components/reload-button/ReloadButton";
import WhitelistButton from "../../common/components/whitelist-button/WhitelistButton";
import UnwhitelistButton from "../../common/components/unwhitelist-button/UnwhitelistButton";

interface Player {
    name: string;
    guid: string;
    uid: string;
    isVerified: boolean;
    ip: string;
    isBanned: boolean;
    isWhitelisted: boolean;
    bannedReasons: string;
}

interface Dictionary<T>{
    [key: string]: T;
}

export default function PlayerDatabase() {

    const [players, setPlayers] = React.useState<Player[]>([]);
    const [unsortedPlayers, setUnsortedPlayers] = React.useState<Player[]>([]);
    const [contextualMenuProps, setContextualMenuProps] = React.useState<IContextualMenuProps>();
    const [sortKey, setSortKey] = React.useState("");
    const [isSortedDescending, setIsSortedDescending] = React.useState(false);
    const [fieldFilters, setFieldFilters] = React.useState<Dictionary<string>>({ "guid": "", "name": "", "ip": "", "bannedReasons": "", "uid": "" });
    //const [groupedKey, setGroupedKey] = React.useState("");

    React.useEffect(() => {
        handleLoad();
    }, []);

    initializeIcons();

    const handleLoad = () => {
        getAllPlayers(setPlayers, setUnsortedPlayers, '/Scheduler/GetPlayers');
    }

    const onColumnContextMenu = (column: IColumn | undefined, ev: React.MouseEvent<HTMLElement> | undefined): void => {
        if (column && ev && column.columnActionsMode !== ColumnActionsMode.disabled) {
            setContextualMenuProps(getContextualMenuProps(column, ev));
        }
    }

    const onColumnClick = (ev: React.MouseEvent<HTMLElement>, column: IColumn): void => {
        if (column.columnActionsMode !== ColumnActionsMode.disabled) {
            setContextualMenuProps(getContextualMenuProps(column, ev));
        }
    }

    const columns: IColumn[] = [
        {
            key: 'name',
            name: 'Name',
            fieldName: 'name',
            minWidth: 80,
            isSorted: sortKey === "name",
            isSortedDescending: isSortedDescending,
            isFiltered: fieldFilters["name"] != "",
            onRender: (item: Player) => {
                return <div style={{ display: "flex", flexDirection: "column", justifyContent: "center", alignContent: "center" }}>{item.name}</div>
            },
            onColumnContextMenu: onColumnContextMenu,
            onColumnClick: onColumnClick,
            columnActionsMode: ColumnActionsMode.hasDropdown,
            isResizable: true
        },
        {
            key: 'guid',
            name: 'Guid',
            fieldName: 'guid',
            minWidth: 320,
            data: 'string',
            isSorted: sortKey === "guid",
            isSortedDescending: isSortedDescending,
            isFiltered: fieldFilters["guid"] != "",
            onRender: (item: Player) => {
                return <span style={{ display: "flex", flexDirection: "column", justifyContent: "center", alignContent: "center" }}>{item.guid}</span>
            },
            onColumnContextMenu: onColumnContextMenu,
            onColumnClick: onColumnClick,
            columnActionsMode: ColumnActionsMode.hasDropdown,
            isResizable: true
        },
        {
            key: 'uid',
            name: 'Unique ID',
            fieldName: 'uid',
            minWidth: 320,
            data: 'string',
            isSorted: sortKey === "uid",
            isSortedDescending: isSortedDescending,
            isFiltered: fieldFilters["uid"] != "",
            onRender: (item: Player) => {
                return <span style={{ display: "flex", flexDirection: "column", justifyContent: "center", alignContent: "center" }}>{item.uid}</span>
            },
            onColumnContextMenu: onColumnContextMenu,
            onColumnClick: onColumnClick,
            columnActionsMode: ColumnActionsMode.hasDropdown,
            isResizable: true
        },
        {
            key: 'ip',
            name: 'IP',
            fieldName: 'ip',
            minWidth: 160,
            data: 'string',
            isSorted: sortKey === "ip",
            isSortedDescending: isSortedDescending,
            isFiltered: fieldFilters["ip"] != "",
            onRender: (item: Player) => {
                return <span style={{ display: "flex", flexDirection: "column", justifyContent: "center", alignContent: "center" }}>{item.ip}</span>
            },
            onColumnContextMenu: onColumnContextMenu,
            onColumnClick: onColumnClick,
            columnActionsMode: ColumnActionsMode.hasDropdown,
            isResizable: true
        },
        {
            key: 'isVerified',
            name: 'Is Verified',
            fieldName: 'isVerified',
            minWidth: 120,
            isSorted: sortKey === "isVerified",
            isSortedDescending: isSortedDescending,
            onRender: (item: Player) => {
                return item.isVerified ?
                    <CheckCircleIcon style={{ display: "flex", flexDirection: "column", justifyContent: "center", alignContent: "center", textAlign: "center" }} /> :
                    <UnpublishedIcon style={{ display: "flex", flexDirection: "column", justifyContent: "center", alignContent: "center", textAlign: "center" }} />
            },
            onColumnContextMenu: onColumnContextMenu,
            onColumnClick: onColumnClick,
            columnActionsMode: ColumnActionsMode.hasDropdown,
            isResizable: true
        },
        {
            key: 'kick',
            name: 'Kick',
            fieldName: 'kick',
            minWidth: 80,
            onRender: (item: Player) => {
                return <KickButton guid={item.guid} name={item.name} reload={handleLoad} />
            }
        },
        {
            key: 'isWhitelisted',
            name: 'Is Whitelisted',
            fieldName: 'isWhitelisted',
            minWidth: 120,
            data: 'boolean',
            isSorted: sortKey === "isWhitelisted",
            isSortedDescending: isSortedDescending,
            onRender: (item: Player) => {
                return item.isWhitelisted ?
                    <CheckCircleIcon style={{ display: "flex", flexDirection: "column", justifyContent: "center", alignContent: "center", textAlign: "center" }} /> :
                    <UnpublishedIcon style={{ display: "flex", flexDirection: "column", justifyContent: "center", alignContent: "center", textAlign: "center" }} />
            },
            onColumnContextMenu: onColumnContextMenu,
            onColumnClick: onColumnClick,
            columnActionsMode: ColumnActionsMode.hasDropdown,
            isResizable: true
        },
        {
            key: 'whitelist',
            name: 'Whitelist Actions',
            fieldName: 'whitelist',
            minWidth: 120,
            onRender: (item: Player) => {
                return item.isWhitelisted ?
                    <UnwhitelistButton guid={item.guid} name={item.name} reload={handleLoad} /> :
                    <WhitelistButton guid={item.guid} name={item.name} reload={handleLoad} />
            }
        },
        {
            key: 'isBanned',
            name: 'Is Banned',
            fieldName: 'isBanned',
            minWidth: 120,
            data: 'boolean',
            isSorted: sortKey === "isBanned",
            isSortedDescending: isSortedDescending,
            onRender: (item: Player) => {
                return item.isBanned ?
                    <CheckCircleIcon style={{ display: "flex", flexDirection: "column", justifyContent: "center", alignContent: "center", textAlign: "center" }} /> :
                    <UnpublishedIcon style={{ display: "flex", flexDirection: "column", justifyContent: "center", alignContent: "center", textAlign: "center" }} />
            },
            onColumnContextMenu: onColumnContextMenu,
            onColumnClick: onColumnClick,
            columnActionsMode: ColumnActionsMode.hasDropdown,
            isResizable: true
        },
        {
            key: 'ban',
            name: 'Ban Actions',
            fieldName: 'ban',
            minWidth: 80,
            onRender: (item: Player) => {
                return item.isBanned ?
                    <UnbanButton guid={item.guid} name={item.name} reload={handleLoad} /> :
                    <BanButton guid={item.guid} name={item.name} reload={handleLoad} />

            }
        },
        {
            key: 'bannedReasons',
            name: 'Banned Reasons',
            fieldName: 'bannedReasons',
            minWidth: 320,
            isSorted: sortKey === "bannedReasons",
            isSortedDescending: isSortedDescending,
            isFiltered: fieldFilters["bannedReasons"] != "",
            onRender: (item: Player) => {
                return <div style={{ display: "flex", flexDirection: "column", padding: "0px 0px 0px 0px" }}>{item.isBanned && item.bannedReasons}</div>
            },
            onColumnContextMenu: onColumnContextMenu,
            onColumnClick: onColumnClick,
            columnActionsMode: ColumnActionsMode.hasDropdown,
            isResizable: true
        }
    ]

    const onContextualMenuDismissed = (): void => {
        setContextualMenuProps(undefined);
    }

    const getContextualMenuProps = (column: IColumn, ev: React.MouseEvent<HTMLElement>): IContextualMenuProps => {
        const items: IContextualMenuItem[] = [
            {
                key: 'aToZ',
                name: 'A to Z',
                iconProps: { iconName: 'SortUp' },
                canCheck: true,
                checked: column.isSorted && !column.isSortedDescending,
                onClick: () => {
                    column.isSorted && !column.isSortedDescending ? _onDisableSorting() : _onSortColumn(column, false);
                }
            },
            {
                key: 'zToA',
                name: 'Z to A',
                iconProps: { iconName: 'SortDown' },
                canCheck: true,
                checked: column.isSorted && column.isSortedDescending,
                onClick: () => {
                    column.isSorted && column.isSortedDescending ? _onDisableSorting() : _onSortColumn(column, true);
                }
            }
        ];

        const filters: IContextualMenuItem[] = [
            {
                key: "clearFilter",
                name: "ClearFilter",
                iconProps: { iconName: 'Filter' },
                canCheck: true,
                checked: column.isFiltered,
                onClick: () => {
                    onFilterChange("", column);
                }
            },
            {
                key: 'filter',
                name: 'Filter',
                iconProps: { iconName: 'Filter' },
                canCheck: false,
                onRender: () => {
                    return <TextField label="Filter" defaultValue={fieldFilters[column.key]} onChange={(event) => onFilterChange(event.currentTarget.value, column)} />
                }
            }
        ]

        if (column.key != "isBanned" && column.key != "isVerified" && column.key != "isWhitelisted") {
            filters.forEach(filter => items.push(filter));
        }

        return {
            items: items,
            target: ev.currentTarget as HTMLElement,
            directionalHint: DirectionalHint.bottomLeftEdge,
            gapSpace: 0,
            isBeakVisible: true,
            onDismiss: onContextualMenuDismissed,
        }
    }

    const _onSortColumn = (column: IColumn, isSortedDescending: boolean) => {
        const sortedPlayers = _copyAndSort<Player>(players, column.key, isSortedDescending);
        setPlayers(sortedPlayers);

        setSortKey(column.key);
        setIsSortedDescending(isSortedDescending);
    };

    const _onDisableSorting = () => {
        setPlayers(unsortedPlayers);

        setSortKey("");
        setIsSortedDescending(false);
    }

    const onFilterChange = (newFilter: string, column: IColumn) => {

        const newFieldFilters = {
            ...fieldFilters,
            [column.key]: newFilter
        };

        setFieldFilters(newFieldFilters);

        const filteredPlayers = unsortedPlayers.filter(
            player => {
                return getValidForFilters(player, newFieldFilters);
            }
        );

        let filteredAndSortedPlayers;
        if (sortKey) {
            filteredAndSortedPlayers = _copyAndSort<Player>(filteredPlayers, sortKey, isSortedDescending);
        }
        else {
            filteredAndSortedPlayers = filteredPlayers;
        }

        setPlayers(
            filteredAndSortedPlayers
        );
    }

    const getValidForFilters = (player: Player, newFieldFilters: Dictionary<string>) => {
        for (let key in player) {
            const playerKey = key as keyof typeof player;
            if (!key || (newFieldFilters[key] && newFieldFilters[key] != "") && !player[playerKey].toString().toLowerCase().includes(newFieldFilters[key].toLowerCase())) {
                return false;
            }
        }
        return true;
    }

    return (
        <div style={{ display: "flex", flexDirection: "column", padding: "10px 10px 10px 10px", flexGrow: 1 }}>
            <div>
                <ReloadButton
                    handleLoad={handleLoad}
                />
            </div>
            <ShimmeredDetailsList
                items={players || []}
                columns={columns}
                selectionMode={SelectionMode.none}
                enableUpdateAnimations
                enableShimmer={players == null}
            />
            {contextualMenuProps && <ContextualMenu {...contextualMenuProps} />}
        </div>
    )
}

async function getAllPlayers(setPlayers: Function, setUnsortedPlayers: Function, endpoint: string) {
    try {
        const response = await fetch(endpoint);
        if (response.status == 200) {
            const result = (await response.json()) as Player[];
            setPlayers(result);
            setUnsortedPlayers(result);
        }
    }
    catch (ex) {
        if (typeof ex === "string") {
            alert(ex);
        }
        else if (ex instanceof Error) {
            alert(ex.message);
        }
    }
}

function _copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
    const key = columnKey as keyof T;
    return items.slice(0).sort((a: T, b: T) => ((isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1));
}