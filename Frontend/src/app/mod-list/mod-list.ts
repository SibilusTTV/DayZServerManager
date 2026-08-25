import {Component, signal, WritableSignal} from '@angular/core';
import {Mod, ModService} from '../../api';
import {type AutoSizeStrategy, ColDef} from 'ag-grid-community';
import {ActivatedRoute, Router} from '@angular/router';
import {AgGridAngular} from 'ag-grid-angular';
import {GridActionsCell} from './grid-actions-cell/grid-actions-cell';

@Component({
  selector: 'app-mod-list',
  templateUrl: './mod-list.html',
  imports: [
    AgGridAngular
  ]
})
export default class ModList {
  public allMods: WritableSignal<Mod[]> = signal([]);

  public colDefs: ColDef[] = [
    {
      field: "name"
    },
    {
      field: "workshopID"
    },
    {
      headerName: "Actions",
      field: "id",
      cellRenderer: GridActionsCell,
      cellRendererParams: {
        reloadManager: this.getMods.bind(this)
      },
      resizable: false,
      maxWidth: 100
    }
  ];

  public autoSizeStrategy: AutoSizeStrategy ={
    type: "fitGridWidth"
  }

  constructor(private route: ActivatedRoute) {
    this.getMods();
  }

  public getMods(){
    ModService.getApiModGetMods().then((data: Mod[]) => {
      this.allMods.set(data);
    })
  }
}
