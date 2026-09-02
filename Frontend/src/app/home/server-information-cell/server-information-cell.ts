import {Component, OnDestroy, OnInit, signal, WritableSignal} from '@angular/core';
import {ICellRendererAngularComp} from 'ag-grid-angular';
import {ICellRendererParams} from 'ag-grid-community';
import {InstanceService, ServerInformation} from '../../../api';

@Component({
  selector: 'server-information-cell',
  templateUrl: './server-information-cell.html',
})
export class ServerInformationCell implements ICellRendererAngularComp, OnInit, OnDestroy {
  public id: number = 0;
  public serverInformation: WritableSignal<ServerInformation> = signal({});
  public timer = 0;

  agInit(params: ICellRendererParams) {
    this.id = params.valueFormatted ? params.valueFormatted : params.value;
  }

  refresh(params: ICellRendererParams) {
    this.id = params.valueFormatted ? params.valueFormatted : params.value;
    return true;
  }

  ngOnInit() {
    InstanceService.getApiInstanceGetServerInformation(this.id).then(serverInformation => {
      this.serverInformation.set(serverInformation);
    });

    this.timer = setInterval(() => {
      InstanceService.getApiInstanceGetServerInformation(this.id).then(serverInformation => {
        this.serverInformation.set(serverInformation);
      });
    }, 5000);
  }

  ngOnDestroy() {
    clearInterval(this.timer);
  }

  public GetServerInformation() {
    const serverInformation = this.serverInformation();
    if (serverInformation.dayzServerStatus != null && serverInformation.playersCount != null && serverInformation.dayzServerStatus == 'Running') {
      return serverInformation.dayzServerStatus + " with " + serverInformation.playersCount + " Players";
    }
    if (serverInformation.dayzServerStatus != null && serverInformation.dayzServerStatus != '') {
      return serverInformation.dayzServerStatus;
    }
    return "Not Running";
  }
}
