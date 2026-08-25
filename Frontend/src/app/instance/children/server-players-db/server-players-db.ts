import {Component, signal, WritableSignal} from '@angular/core';
import {PlayerService, ServerPlayerInformation} from '../../../../api';
import {ActivatedRoute} from '@angular/router';
import {AgGridAngular} from 'ag-grid-angular';
import {ColDef} from 'ag-grid-community';
import type {AutoSizeStrategy} from 'ag-grid-community';
import {PlayersActionsCell} from './players-actions-cell/players-actions-cell';


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

  public colDefs: ColDef[] = [
    {
      field: 'id',
      headerName: "Guid"
    },
    {
      field: 'name'
    },
    {
      field: 'uid'
    },
    {
      field: 'isVerified'
    },
    {
      field: 'ip'
    },
    {
      field: 'isWhitelisted'
    },
    {
      field: 'isBanned'
    },
    {
      field: 'role'
    },
    {
      field: 'serverPlayerId',
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
      PlayerService.getApiPlayerGetServerPlayerInformation(this.id).then(serverPlayerInformations => {
        this.serverPlayerInformations.set(serverPlayerInformations);
      })
    });
  }

  public Reload(): void {
    PlayerService.getApiPlayerGetServerPlayerInformation(this.id).then(serverPlayerInformations => {
      this.serverPlayerInformations.set(serverPlayerInformations);
    })
  }
}
