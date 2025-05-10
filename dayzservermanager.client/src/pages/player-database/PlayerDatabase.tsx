

import * as React from "react";
import KickButton from "../../common/components/kick-button/KickButton";
import UnbanButton from "../../common/components/unban-button/UnbanButton";
import BanButton from "../../common/components/ban-button/BanButton";
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import UnpublishedIcon from '@mui/icons-material/Unpublished';
import { ColumnActionsMode, ContextualMenu, DirectionalHint, IColumn, IContextualMenuItem, IContextualMenuProps, initializeIcons, SelectionMode, ShimmeredDetailsList, TextField } from "@fluentui/react";
import ReloadButton from "../../common/components/reload-button/ReloadButton";

interface PlayersDB {
    players: Player[];
}

interface Player {
    name: string;
    guid: string;
    isVerified: boolean;
    ip: string;
}

interface BannedPlayer {
    banId: number;
    guid: string;
    remainingTime: number;
    reason: string;
}

interface ListItem {
    name: string;
    guid: string;
    isVerified: boolean;
    ip: string;
    isBanned: boolean;
    bannedReasons: string;
}

interface Dictionary<T>{
    [key: string]: T;
}

export default function PlayerDatabase() {

    const [listItems, setListItems] = React.useState<ListItem[]>([]);
    const [unsortedListItems, setUnsortedListItems] = React.useState<ListItem[]>([]);
    const [contextualMenuProps, setContextualMenuProps] = React.useState<IContextualMenuProps>();
    const [sortKey, setSortKey] = React.useState("");
    const [isSortedDescending, setIsSortedDescending] = React.useState(false);
    const [fieldFilters, setFieldFilters] = React.useState<Dictionary<string>>({ "guid": "", "name": "", "ip": "", "bannedReasons": "", "isBanned": "", "isVerified": "" });
    //const [groupedKey, setGroupedKey] = React.useState("");

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

    initializeIcons();

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
            key: 'isVerified',
            name: 'Is Verified',
            fieldName: 'isVerified',
            minWidth: 120,
            isSorted: sortKey === "isVerified",
            isSortedDescending: isSortedDescending,
            isFiltered: fieldFilters["isVerified"] != "",
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
            key: 'isBanned',
            name: 'Is Banned',
            fieldName: 'isBanned',
            minWidth: 120,
            data: 'boolean',
            isSorted: sortKey === "isBanned",
            isSortedDescending: isSortedDescending,
            isFiltered: fieldFilters["isBanned"] != "",
            onRender: (item: ListItem) => {
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
            key: 'kick',
            name: 'Kick',
            fieldName: 'kick',
            minWidth: 80,
            onRender: (item: Player) => {
                return <KickButton guid={item.guid} name={item.name} />
            }
        },
        {
            key: 'ban',
            name: 'Ban',
            fieldName: 'ban',
            minWidth: 80,
            onRender: (item: Player) => {
                return <BanButton guid={item.guid} name={item.name} />
            }
        },
        {
            key: 'unban',
            name: 'Unban',
            fieldName: 'unban',
            minWidth: 80,
            onRender: (item: Player) => {
                return <UnbanButton guid={item.guid} name={item.name} />
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
            onRender: (item: ListItem) => {
                return <div style={{ display: "flex", flexDirection: "column", padding: "0px 0px 0px 0px" }}>{item.isBanned && item.bannedReasons}</div>
            },
            onColumnContextMenu: onColumnContextMenu,
            onColumnClick: onColumnClick,
            columnActionsMode: ColumnActionsMode.hasDropdown,
            isResizable: true
        }
    ]

    React.useEffect(() => {
        getAllPlayers(setListItems, setUnsortedListItems, 'SchedulerConfig/GetPlayers');
    }, []);

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
                checked: column.isSorted,
                onClick: () => {
                    onFilterChange("", column);
                }
            },
            {
                key: 'filter',
                name: 'Filter',
                iconProps: { iconName: 'Filter' },
                canCheck: false,
                checked: column.isSorted,
                onRender: () => {
                    return <TextField label="Filter" defaultValue={fieldFilters[column.key]} onChange={(event) => onFilterChange(event.currentTarget.value, column)} />
                }
            }
        ]

        if (column.key != "isBanned" && column.key != "isVerified") {
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
        const sortedListItems = _copyAndSort<ListItem>(listItems, column.key, isSortedDescending);
        setListItems(sortedListItems);

        setSortKey(column.key);
        setIsSortedDescending(isSortedDescending);
    };

    const _onDisableSorting = () => {
        setListItems(unsortedListItems);

        setSortKey("");
        setIsSortedDescending(false);
    }

    const onFilterChange = (newFilter: string, column: IColumn) => {

        const newFieldFilters = {
            ...fieldFilters,
            [column.key]: newFilter
        };

        setFieldFilters(newFieldFilters);

        setListItems(
            unsortedListItems.filter(
                listItem => {
                    return getValidForFilters(listItem, newFieldFilters);
                }
            )
        );
    }

    const getValidForFilters = (listItem: ListItem, newFieldFilters: Dictionary<string>) => {
        for (let filterKey in newFieldFilters) {
            const key: keyof typeof listItem = Object.keys(listItem).find(key => key.valueOf() == filterKey) as keyof typeof listItem;
            if (!key || newFieldFilters[filterKey] != "" && !listItem[key].toString().toLowerCase().includes(newFieldFilters[filterKey].toLowerCase())) {
                return false;
            }
        }
        return true;
    }

    return (
        <div style={{ display: "flex", flexDirection: "column", padding: "10px 10px 10px 10px", flexGrow: 1 }}>
            <div>
                <ReloadButton
                    populateFunction={getAllPlayers}
                    setFunction={setListItems}
                    setFunctionUnsorted={setUnsortedListItems}
                />
            </div>
            <ShimmeredDetailsList
                items={listItems || []}
                columns={columns}
                selectionMode={SelectionMode.none}
                enableUpdateAnimations
                enableShimmer={listItems == null}
            />
            {contextualMenuProps && <ContextualMenu {...contextualMenuProps} />}
        </div>
    )
}

async function getAllPlayers(setListItems: Function, setUnsortedListItems: Function, endpoint: string) {
    let players: Player[] = [];
    try {
        const response = await fetch('SchedulerConfig/GetPlayers');
        if (response.status == 200) {
            const result = (await response.json()) as PlayersDB;
            players = result.players;
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

    let bannedPlayers: BannedPlayer[];
    try {
        const response = await fetch('SchedulerConfig/GetBannedPlayers');
        if (response.status == 200) {
            const result = (await response.json()) as BannedPlayer[];
            bannedPlayers = result;
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

    if (players) {
        let listItems: ListItem[] = players.map(player => {
            let bannedReasons = "";
            (bannedPlayers) && bannedPlayers.forEach(bannedPlayer => { if (bannedPlayer.guid === player.guid) { bannedReasons += bannedPlayer.reason + "\n" } })
            const listItem: ListItem = {
                name: player.name,
                guid: player.guid,
                isVerified: player.isVerified,
                ip: player.ip,
                isBanned: bannedReasons != "",
                bannedReasons: bannedReasons
            }
            return listItem;
        });

        setListItems(listItems);
        setUnsortedListItems(listItems);
    }
}

function _copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
    const key = columnKey as keyof T;
    return items.slice(0).sort((a: T, b: T) => ((isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1));
}