import {Component, signal, WritableSignal} from '@angular/core';
import {AgGridAngular} from 'ag-grid-angular';
import {type AutoSizeStrategy, ColDef} from 'ag-grid-community';
import {PlayerService, User} from '../../api';


@Component({
  selector: 'players-db',
  imports: [
    AgGridAngular
  ],
  templateUrl: 'players-db.html'
})
export default class PlayersDb {
  public players: WritableSignal<User[]> = signal([]);

  public colDefs: ColDef[] = [
    {
      field: "name"
    },
    {
      field: "guid"
    },
    {
      field: "uid"
    },
    {
      field: "isVerified"
    },
    {
      field: "ip"
    },
  ]

  public autoSizeStrategy: AutoSizeStrategy ={
    type: "fitGridWidth"
  }

  constructor() {
    PlayerService.getApiPlayerGetPlayers().then((response) => {
      this.players.set(response);
    });
  }
}
