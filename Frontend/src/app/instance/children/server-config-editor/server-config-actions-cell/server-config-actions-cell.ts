import {Component} from '@angular/core';
import {ICellRendererAngularComp} from 'ag-grid-angular';
import {ICellRendererParams} from 'ag-grid-community';
import {MatIcon} from '@angular/material/icon';
import {MatIconButton} from '@angular/material/button';

@Component({
  selector: 'server-config-actions-cell',
  templateUrl: './server-config-actions-cell.html',
  imports: [
    MatIcon,
    MatIconButton
  ]
})
export default class ServerConfigActionsCell implements ICellRendererAngularComp {
  private params: any;
  private id: string = "";

  agInit(params: ICellRendererParams) {
    this.params = params;
  }

  refresh(params: ICellRendererParams) {
    this.params = params;
    return true;
  }

  public onDeleteClick() {
    this.params.onDeleteClick(this.params.data.propertyName, this.params.data.value, this.params.data.comment);
  }
}
