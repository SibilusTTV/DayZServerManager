import {Component} from '@angular/core';
import {ICellRendererAngularComp} from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';
import {MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {InstanceService} from '../../../api';


@Component({
  selector: 'server-control-cell',
  templateUrl: './server-control-cell.html',
  imports: [
    MatIconButton,
    MatIcon
  ]
})
export default class ServerControlCell implements ICellRendererAngularComp{
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

  public onStartClicked(){
    InstanceService.getApiInstanceStartServer(this.id).then();
  }

  public onStopClicked(){
    InstanceService.getApiInstanceStopServer(this.id).then();
  }

  public onRemoveClicked(){
    InstanceService.deleteApiInstanceRemoveServer(this.id).then(() => {
      this.params.reloadManager();
    });
  }
}
