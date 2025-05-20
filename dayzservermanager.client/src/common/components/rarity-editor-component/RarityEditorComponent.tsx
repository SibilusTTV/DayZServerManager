
import { useEffect, useMemo, useState } from "react";
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/DeleteOutlined';
import "./RarityEditorComponent.css";
import SaveButton from "../save-button/SaveButton";
import ReloadButton from "../reload-button/ReloadButton";
import { CheckboxVisibility, ColumnActionsMode, ContextualMenu, DefaultButton, DetailsList, DirectionalHint, Dropdown, IColumn, IContextualMenuItem, IContextualMenuProps, IDropdownOption, initializeIcons, IObjectWithKey, Selection, SelectionMode, TextField } from "@fluentui/react";

interface RarityFile {
    itemRarity: RarityItem[];
}

interface RarityItem {
    id: number,
    name: string,
    rarity: number
}

interface RarityEditorProps {
    name: string;
}

interface Dictionary<T> {
    [key: string]: T;
}

const rarityDropdownOptions: IDropdownOption[] = [
    {
        key: 0,
        text: "Empty"
    },
    {
        key: 1,
        text: "Poor"
    },
    {
        key: 2,
        text: "Common"
    },
    {
        key: 3,
        text: "Uncommon"
    },
    {
        key: 4,
        text: "Rare"
    },
    {
        key: 5,
        text: "Epic"
    },
    {
        key: 6,
        text: "Legendary"
    },
    {
        key: 7,
        text: "Mythic"
    },
    {
        key: 8,
        text: "Exotic"
    }
]

export default function RarityEditor(props: RarityEditorProps) {
    const [rarities, setRarities] = useState<RarityFile>();
    const [sortedRarities, setSortedRarities] = useState<RarityFile>();
    const [checkedItems, setCheckedItems] = useState<IObjectWithKey[]>([]);
    const [contextualMenuProps, setContextualMenuProps] = useState<IContextualMenuProps>();
    const [sortKey, setSortKey] = useState("");
    const [isSortedDescending, setIsSortedDescending] = useState(false);
    const [fieldFilters, setFieldFilters] = useState<Dictionary<string>>({ "id": "", "name": "", "rarity": "" });

    initializeIcons();

    useEffect(() => {
        handleLoad();
    }, []);

    const rarityItemSelection: Selection = useMemo(() => new Selection(
        {
            onSelectionChanged: () => {
                setCheckedItems(rarityItemSelection.getSelection());
            },
            selectionMode: SelectionMode.multiple,
            getKey: (item) => (item as RarityItem).id
        }),
        []
    );

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
            key: "name",
            fieldName: "name",
            name: "Name",
            minWidth: 360,
            isSorted: sortKey === "name",
            isSortedDescending: isSortedDescending,
            isFiltered: fieldFilters["name"] != "",
            onRender: (item: RarityItem) => {
                return (
                    <TextField
                        id={"name-" + item.id}
                        value={item.name}
                        onChange={(_, newValue) => handleFieldChange(newValue, item, "name")}
                    />
                )
            },
            onColumnContextMenu: onColumnContextMenu,
            onColumnClick: onColumnClick,
            columnActionsMode: ColumnActionsMode.hasDropdown,
            isResizable: true
        },
        {
            key: 'rarity',
            fieldName: 'rarity',
            name: 'Rarity',
            minWidth: 120,
            isSorted: sortKey === "raritiy",
            isSortedDescending: isSortedDescending,
            isFiltered: fieldFilters["rarity"] != "",
            onRender: (item: RarityItem) => {
                return (
                    <Dropdown
                        id={"name-" + item.id}
                        selectedKey={item.rarity}
                        options={rarityDropdownOptions}
                        onChange={(_, option) => handleFieldChange(option?.key, item, "rarity")}
                    />
                )
            },
            onColumnContextMenu: onColumnContextMenu,
            onColumnClick: onColumnClick,
            columnActionsMode: ColumnActionsMode.hasDropdown,
            isResizable: true
        },
        {
            key: 'delete',
            fieldName: 'delete',
            name: 'Delete',
            minWidth: 80,
            onRender: (item: RarityItem) => {
                return (
                    <DefaultButton
                        onClick={() => deleteRarity(item.id)}
                        label="Delete"
                        className="Button"
                    >
                        <DeleteIcon />
                    </DefaultButton>
                )
            }
        }
    ];

    const handleLoad = () => {
        PopulateRarityFile(setSortedRarities, setRarities, '/RarityEditor/GetRarityFile/' + props.name);
    }

    const handleSave = () => {
        PostRarityFile('/RarityEditor/PostRarityFile/' + props.name, JSON.stringify(rarities));
    }

    const getNewId = (array: RarityItem[]) => {
        let newId: number = 0;
        let idArray: number[] = [];
        array.map((item) => { idArray.push(item.id) })

        for (let i: number = 0; i < idArray.length; i++) {
            if (idArray.find(id => id === i) === undefined) {
                return i;
            }
            else {
                newId++;
            }
        }
        return newId;
    }

    const handleAddClick = () => {
        if (sortedRarities && rarities) {
            const newRarity: RarityItem = { id: getNewId(sortedRarities.itemRarity), name: "", rarity: 0 };

            setSortedRarities(
                {
                    ...sortedRarities,
                    itemRarity: [
                        ...sortedRarities.itemRarity,
                        newRarity
                    ]
                }
            );

            setRarities(
                {
                    ...rarities,
                    itemRarity: [
                        ...rarities.itemRarity,
                        newRarity
                    ]
                }
            );
        }
    };

    const handleBulkChangeClick = (_value: number) => {
        if (sortedRarities && rarities) {
            setSortedRarities(
                {
                    ...sortedRarities,
                    itemRarity: sortedRarities.itemRarity.map(item => (checkedItems as RarityItem[]).find(selectedItem => selectedItem.id === item.id) ? { ...item, rarity: _value } : item)
                }
            );
            setRarities(
                {
                    ...rarities,
                    itemRarity: rarities.itemRarity.map(item => (checkedItems as RarityItem[]).find(selectedItem => selectedItem.id === item.id) ? { ...item, rarity: _value } : item)
                }
            );
        }
    }

    const handleFieldChange = (newValue: string | number | undefined, rarityItem: RarityItem, key: string) => {
        if (sortedRarities && rarities) {
            const updatedRarityItem = { ...rarityItem, [key]: newValue };
            setSortedRarities(
                {
                    ...sortedRarities,
                    itemRarity: sortedRarities.itemRarity.map((row) => (row.id === updatedRarityItem.id ? updatedRarityItem : row))
                }
            );
            setRarities(
                {
                    ...rarities,
                    itemRarity: rarities.itemRarity.map((row) => (row.id === updatedRarityItem.id ? updatedRarityItem : row))
                }
            );
        }
    };

    const deleteRarity = (id: number) => {
        if (sortedRarities && rarities) {
            setSortedRarities(
                {
                    ...sortedRarities,
                    itemRarity: sortedRarities.itemRarity.filter(x => x.id != id)
                }
            );
            setRarities(
                {
                    ...rarities,
                    itemRarity: rarities.itemRarity.filter(x => x.id != id)
                }
            );
        }
    }

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

        const textfieldFilters: IContextualMenuItem[] = [
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
        ];

        const dropdownFilters: IContextualMenuItem[] = [
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
                    return <Dropdown options={rarityDropdownOptions} label="Filter" defaultSelectedKey={(fieldFilters["rarity"] && fieldFilters["rarity"] != null) ? Number(fieldFilters["rarity"]) : undefined} onChange={(_, option) => option && onFilterChange(option.key.toString(), column)} />
                }
            }

        ];

        if (column.key != "rarity") {
            textfieldFilters.forEach(filter => items.push(filter));
        }
        else {
            dropdownFilters.forEach(filter => items.push(filter));
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
        if (sortedRarities) {
            const sortedItemRarities = _copyAndSort<RarityItem>(sortedRarities.itemRarity, column.key, isSortedDescending);
            setSortedRarities({ ...sortedRarities, itemRarity: sortedItemRarities });

            setSortKey(column.key);
            setIsSortedDescending(isSortedDescending);
        }
    };

    const _onDisableSorting = () => {
        setSortedRarities(rarities);

        setSortKey("");
        setIsSortedDescending(false);
    }

    const onFilterChange = (newFilter: string, column: IColumn) => {
        if (rarities) {
            const newFieldFilters = {
                ...fieldFilters,
                [column.key]: newFilter
            };

            setFieldFilters(newFieldFilters);

            const filteredItemRarities = rarities.itemRarity.filter(
                player => {
                    return getValidForFilters(player, newFieldFilters);
                }
            );

            let filteredAndSortedItemRarities;
            if (sortKey) {
                filteredAndSortedItemRarities = _copyAndSort<RarityItem>(filteredItemRarities, sortKey, isSortedDescending);
            }
            else {
                filteredAndSortedItemRarities = filteredItemRarities;
            }

            setSortedRarities({
                ...rarities,
                itemRarity: filteredAndSortedItemRarities
            });
        }
    }

    const getValidForFilters = (rarity: RarityItem, newFieldFilters: Dictionary<string>) => {
        for (let key in rarity) {
            const rarityKey = key as keyof typeof rarity;
            if (!key || (newFieldFilters[key] && newFieldFilters[key] != "") && !rarity[rarityKey].toString().toLowerCase().includes(newFieldFilters[key].toLowerCase())) {
                return false;
            }
        }
        return true;
    }

    const contents = sortedRarities == null
        ? <p><em>Either the backend hasn't loaded yet or the Rarity File is empty</em></p>
        :
        <DetailsList
            setKey={props.name + "DetailsList"}
            items={sortedRarities.itemRarity}
            columns={columns}
            selection={rarityItemSelection}
            checkboxVisibility={CheckboxVisibility.always}
        />


    return (
        <div style={{padding: "10px 10px 10px 10px"} }>
            <h1 id="tableLabel">{props.name}</h1>
            <div id="SaveButton" style={{ display: "flex", flexDirection: "row", gap: "10px" }}>
                <SaveButton
                    handleSave={handleSave}
                />
                <ReloadButton
                    handleLoad={handleLoad}
                />
            </div>
            <div id="Contents">
                <div style={{ display: "flex", flexDirection: "row", padding: "20px 0px 0px 0px", gap: "10px" }}>
                    <DefaultButton
                        onClick={handleAddClick}
                        className="Button"
                    >
                        <AddIcon />
                    </DefaultButton>
                    {rarityDropdownOptions.map((value) => <DefaultButton className="Button" onClick={() => handleBulkChangeClick(Number(value.key))}>{value.text} ({String(value.key)})</DefaultButton>)}
                </div>
                {contents}
                {contextualMenuProps && <ContextualMenu {...contextualMenuProps} />}
            </div>
        </div>
    )
}

async function PopulateRarityFile(setSortedRarities: Function, setRarities: Function, endpoint: string) {
    const response = await fetch(endpoint);
    if (response.status == 200) {
        const result = (await response.json()) as RarityFile;
        if (result != null) {
            setRarities(result);
            setSortedRarities(result);
        }
    }
}

async function PostRarityFile(endpoint: string, data: string) {
    const response = await fetch(endpoint, {
        method: "POST",
        mode: "cors",
        headers: { 'Content-Type': 'application/json' },
        body: data
    });
    const result = await response.text();
    if (result != null) {
        alert(result);
    }
}

function _copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
    const key = columnKey as keyof T;
    return items.slice(0).sort((a: T, b: T) => ((isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1));
}