import {Component, inject} from '@angular/core';
import {ICellRendererAngularComp} from 'ag-grid-angular';
import {ICellRendererParams} from 'ag-grid-community';
import {MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {InstanceService} from '../../../../../api';
import {MatDialog} from '@angular/material/dialog';
import {KickWindow} from '../../../../../components/kick-window/kick-window';
import {BanWindow} from '../../../../../components/ban-window/ban-window';

@Component({
  selector: 'overview-actions-cell',
  imports: [
    MatIconButton,
    MatIcon
  ],
  templateUrl: 'overview-actions-cell.html'
})
export default class OverviewActionsCell implements ICellRendererAngularComp  {
  public params: any;
  private readonly dialog = inject(MatDialog);

  agInit(params: ICellRendererParams) {
    this.params = params;
  }

  refresh(params: ICellRendererParams) {
    this.params = params;
    return true;
  }

  public onKickClick() {
    const dialogRef = this.dialog.open(KickWindow, {
      data: {
        guid: this.params.value,
        instanceId: this.params.instanceId
      }
    });

    dialogRef.afterClosed().subscribe(result => {
    })
  }

  public onBanClick(){
    const dialogRef = this.dialog.open(BanWindow, {
      data: {
        guid: this.params.value,
        instanceId: this.params.instanceId
      }
    });

    dialogRef.afterClosed().subscribe(result => {
    })
  }
}
