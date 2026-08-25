import {Component} from '@angular/core';
import {ICellRendererAngularComp} from 'ag-grid-angular';
import {ICellRendererParams} from 'ag-grid-community';
import {InstanceService, ModService} from '../../../api';
import {MatIcon} from '@angular/material/icon';
import {MatIconButton} from '@angular/material/button';

@Component({
  selector: 'grid-actions-cell',
  templateUrl: './grid-actions-cell.html',
  imports: [
    MatIcon,
    MatIconButton
  ]
})
export class GridActionsCell implements ICellRendererAngularComp {
  public id: string = "";
  private params: any;

  agInit(params: ICellRendererParams) {
    this.params = params;
    this.id = params.valueFormatted ? params.valueFormatted : params.value;
  }

  refresh(params: ICellRendererParams) {
    this.params = params;
    this.id = params.valueFormatted ? params.valueFormatted : params.value;
    return true;
  }

  public onRemoveClicked(){
    ModService.deleteApiModDeleteMod(this.id).then(() => {
      this.params.reloadManager();
    });
  }
}
