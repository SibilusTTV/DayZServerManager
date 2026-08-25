import {Component} from '@angular/core';
import {ICellRendererParams} from 'ag-grid-community';
import {PlayerService, SchedulerService} from '../../../../../api';
import {MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {ICellRendererAngularComp} from 'ag-grid-angular';

@Component({
  selector: 'players-actions-cell',
  templateUrl: './players-actions-cell.html',
  imports: [
    MatIconButton,
    MatIcon
  ]
})
export class PlayersActionsCell implements ICellRendererAngularComp  {
  public serverPlayerId: string = "";
  private params: any;

  agInit(params: ICellRendererParams) {
    this.params = params;
    console.log(params);
    this.serverPlayerId = params.valueFormatted ? params.valueFormatted : params.value;
  }

  refresh(params: ICellRendererParams) {
    this.params = params;
    this.serverPlayerId = params.valueFormatted ? params.valueFormatted : params.value;
    return true;
  }

  public onWhitelistClicked(){
    console.log(this.serverPlayerId)
    if (this.serverPlayerId == null){

      PlayerService.postApiPlayerCreateServerPlayer(this.params.data.id, this.params.instanceId, true, false).then(() => {
        this.params.reload();
      });
    }
    else{
      SchedulerService.getApiSchedulerWhitelistPlayer(this.serverPlayerId, "1").then(() => {
        this.params.reload();
      });
    }
  }

  public onBanClicked(){
    if (this.serverPlayerId == null){
      PlayerService.postApiPlayerCreateServerPlayer(this.params.data.id, this.params.instanceId, false, true).then(() => {
        this.params.reload();
      });
    }
    else{
      SchedulerService.getApiSchedulerBanPlayer(this.serverPlayerId, "1", 1).then(() => {
        this.params.reload();
      });
    }
  }
}
