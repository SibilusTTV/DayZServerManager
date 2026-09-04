import {Component, signal, WritableSignal} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {type AutoSizeStrategy, ColDef, RowSelectionOptions} from 'ag-grid-community';
import {AgGridAngular} from 'ag-grid-angular';
import {RarityItem, RarityService} from '../../../../api';
import {MatButton, MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {RarityActionsCell} from './rarity-actions-cell/rarity-actions-cell';

@Component({
  selector: 'rarity-editor',
  templateUrl: './rarity-editor.html',
  imports: [
    AgGridAngular,
    MatButton,
    MatIconButton,
    MatIcon
  ]
})
export class RarityEditor {
  private id: number = 0;
  private fileName: string = "";
  public rarities: WritableSignal<RarityItem[]> = signal([]);
  private selectedRowIds: number[] = [];

  public ColDefs: ColDef[] = [
    {
      field: "id",
      maxWidth: 80
    },
    {
      field: "name",
      editable: true,
      filter: true
    },
    {
      field: "rarity",
      editable: true,
      cellEditor: "agSelectCellEditor",
      cellEditorParams: {
        values: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]
      },
      filter: true
    },
    {
      field: "id",
      cellRenderer: RarityActionsCell,
      cellRendererParams: (params: any) => ({
        onDeleteClick: this.onDeleteClick.bind(this),
        params
      })
    }
  ];

  public autoSizeStrategy: AutoSizeStrategy ={
    type: "fitGridWidth"
  }

  public rowSelection: RowSelectionOptions = {
    mode: 'multiRow',
    checkboxes: true,
    headerCheckbox: true,
    enableClickSelection: false,
    selectAll: 'filtered'
  };

  constructor(private route: ActivatedRoute) {
    this.route.params.subscribe(params => {
      this.id = params["id"];
      this.fileName = params["rarity"] + ".json";
      RarityService.getApiRarityGet(this.id, this.fileName).then((rarity) => {
        this.rarities.set(rarity?.itemRarity ?? []);
      })
    });
  }

  public onSelectionChange(event: any){
    this.selectedRowIds = event.selectedNodes.map((node: any) => node.data.id);
  }

  public onBulkEditClick(rarity: number){
    this.rarities.set(this.rarities().map(x => {
      if (this.selectedRowIds.indexOf(x.id ?? -1) >= 0){
        return { ...x, rarity: rarity };
      }
      else{
      console.log(this.selectedRowIds.indexOf(x.id ?? -1));
        return x;
      }
    }));
  }

  public onAddClick(){
    this.rarities.set([...this.rarities(),{
      id: this.getNextId(),
      name: "newRarity",
      rarity: 0
    }])
  }

  public onDeleteClick(id: number){
    const filteredItems = this.rarities().filter(x => x.id != id);
    this.rarities.set(filteredItems);
  }

  private getNextId(){
    let i: number = 0
    for (i; i < this.rarities().length; i++){
      if (this.rarities().filter(x => x.id == i).length <= 0){
        return i;
      }
    }
    return i;
  }
}
