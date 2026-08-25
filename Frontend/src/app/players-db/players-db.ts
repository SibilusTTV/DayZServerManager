import {Component} from '@angular/core';
import {AgGridAngular} from 'ag-grid-angular';
import {ColDef} from 'ag-grid-community';
import {Player, PlayerService} from '../../api';


@Component({
  selector: 'players-db',
  imports: [
    AgGridAngular
  ],
  templateUrl: 'players-db.html'
})
export default class PlayersDb {
  public players: Player[] = [];

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


  constructor() {
    PlayerService.getApiPlayerGetPlayers().then((response) => {
      this.players = response;
    });
  }
}
