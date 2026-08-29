import {Component, signal, WritableSignal} from '@angular/core';
import {PlayerService, ServerPlayerInformation} from '../../../../api';
import {ActivatedRoute} from '@angular/router';
import {AgGridAngular} from 'ag-grid-angular';
import {ColDef, ISelectCellEditorParams} from 'ag-grid-community';
import type {AutoSizeStrategy, CellEditingStoppedEvent} from 'ag-grid-community';
import {PlayersActionsCell} from './players-actions-cell/players-actions-cell';
import {MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {v4} from 'uuid';


@Component({
  selector: 'server-players-db',
  templateUrl: 'server-players-db.html',
  imports: [
    AgGridAngular
  ]
})
export default class ServerPlayersDb {
  public serverPlayerInformations: WritableSignal<ServerPlayerInformation[]> = signal([]);
  public id: string = "";
  public roles: string[] = [];

  public colDefs: ColDef[] = [
    {
      field: 'id',
      headerName: "Guid",
      filter: true
    },
    {
      field: 'name',
      filter: true
    },
    {
      field: 'uid',
      filter: true
    },
    {
      field: 'isVerified',
      filter: true
    },
    {
      field: 'ip',
      filter: true
    },
    {
      field: 'isWhitelisted',
      filter: true
    },
    {
      field: 'isBanned',
      filter: true
    },
    {
      field: 'role',
      cellEditor: "agSelectCellEditor",
      cellEditorParams: this.getCellSelectorArray.bind(this),
      editable: true,
      filter: true
    },
    {
      field: 'id',
      headerName: 'Actions',
      cellRenderer: PlayersActionsCell,
      cellRendererParams: (params: any) => ({
        reload: this.Reload.bind(this),
        instanceId: this.id,
        params
      })
    }
  ];

  public autoSizeStrategy: AutoSizeStrategy ={
    type: "fitGridWidth"
  }

  public constructor(private route: ActivatedRoute){
    this.route.params.subscribe(params => {
      this.id = params["id"];
      this.Reload();
      PlayerService.getApiPlayerGetRoleNames(this.id).then(roleNames => {
        this.roles = ["", ...roleNames];
      })
    });
  }

  public Reload(): void {
    PlayerService.getApiPlayerGetServerPlayerInformation(this.id).then(serverPlayerInformations => {
      this.serverPlayerInformations.set(serverPlayerInformations);
    })
  }

  public getCellSelectorArray(): ISelectCellEditorParams{
    return {
      values: this.roles
    }
  }

  public onChangeRole(event: CellEditingStoppedEvent<ServerPlayerInformation>): void {
    const playerId = event.data?.serverPlayerId ?? v4();
    const instanceId = this.id;
    const roleName = event.data?.role ?? "";
    const playerGuid = event.data?.id ?? "";
    PlayerService.postApiPlayerSetRole(playerId, playerGuid, instanceId, roleName).then(() => this.Reload());
  }
}
