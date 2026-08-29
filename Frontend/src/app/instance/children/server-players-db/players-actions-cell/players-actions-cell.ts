import {Component, inject} from '@angular/core';
import {ICellRendererParams} from 'ag-grid-community';
import {InstanceService, PlayerService} from '../../../../../api';
import {MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {ICellRendererAngularComp} from 'ag-grid-angular';
import {MatDialog} from '@angular/material/dialog';
import {BanWindow} from '../../../../../components/ban-window/ban-window';

@Component({
  selector: 'players-actions-cell',
  templateUrl: './players-actions-cell.html',
  imports: [
    MatIconButton,
    MatIcon
  ]
})
export class PlayersActionsCell implements ICellRendererAngularComp  {
  private playerGuid: string = "";
  public params: any;
  private role: string = "";
  private readonly dialog = inject(MatDialog);

  agInit(params: ICellRendererParams) {
    this.init(params);
  }

  refresh(params: ICellRendererParams) {
    this.init(params);
    return true;
  }

  private init(params: ICellRendererParams) {
    this.params = params;
    this.playerGuid = params.valueFormatted ? params.valueFormatted : params.value;
    PlayerService.getApiPlayerGetRoleNames(this.params.instanceId).then(response => {
      if (response.length <= 0 || response.find(x => x == "everyone") != null){
        this.role = "everyone";
      }
      else {
        this.role = response[1];
      }
    })
  }

  public onWhitelistClicked(){
    if (this.playerGuid == null){
      PlayerService.postApiPlayerCreateServerPlayer(this.params.data.id, this.params.instanceId, true, false, "everyone").then(() => {
        this.params.reload();
      });
    }
    else{
      InstanceService.getApiInstanceWhitelistPlayer(this.playerGuid, this.params.instanceId).then(() => {
        this.params.reload();
      });
    }
  }

  public onBanClicked(){
    if (this.playerGuid == null){
      PlayerService.postApiPlayerCreateServerPlayer(this.params.data.id, this.params.instanceId, false, true, "everyone").then(() => {
        this.params.reload();
      });
    }
    else{
      const dialogRef = this.dialog.open(BanWindow, {
        data: {
          guid: this.playerGuid,
          instanceId: this.params.instanceId
        }
      });

      dialogRef.afterClosed().subscribe(result => {
        this.params.reload();
      })
    }
  }

  public onUnbanClicked(){
    if (this.playerGuid != null && this.params.data != null){
      InstanceService.getApiInstanceUnbanPlayer(this.playerGuid, this.params.instanceId).then(() => {
        this.params.reload();
      })
    }
  }

  public onUnwhitelistClicked(){
    if (this.playerGuid != null && this.params.data != null){
      InstanceService.getApiInstanceUnwhitelistPlayer(this.playerGuid, this.params.instanceId).then(() => {
        this.params.reload();
      })
    }
  }
}
