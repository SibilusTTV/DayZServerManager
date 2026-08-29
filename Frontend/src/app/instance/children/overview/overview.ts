import {Component, inject, OnDestroy, OnInit, signal, WritableSignal} from '@angular/core';
import {InstanceService, ServerInformation} from '../../../../api';
import {ActivatedRoute, Router} from '@angular/router';
import {MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {FormsModule} from '@angular/forms';
import {MatFormField, MatInput, MatLabel} from '@angular/material/input';
import {AgGridAngular} from 'ag-grid-angular';
import {ColDef} from 'ag-grid-community';
import ServerConfigActionsCell from '../server-config-editor/server-config-actions-cell/server-config-actions-cell';
import OverviewActionsCell from './overview-actions-cell/overview-actions-cell';

@Component({
  selector: 'instance-overview',
  imports: [
    MatIcon,
    MatIconButton,
    FormsModule,
    MatFormField,
    MatInput,
    MatLabel,
    AgGridAngular
  ],
  templateUrl: './overview.html',
  styleUrl: './overview.css'
})
export default class Overview implements OnInit, OnDestroy {
  private router = inject(Router);
  public instanceId: string = "";
  public serverInformation: WritableSignal<ServerInformation> = signal({});

  private timer: number = 5;

  public ColDefs: ColDef[] = [
    {
      field: "name"
    },
    {
      field: "guid"
    },
    {
      field: "id"
    },
    {
      field: "ping"
    },
    {
      field: "isVerified"
    },
    {
      field: "isInLobby"
    },
    {
      field: "ip"
    },
    {
      field: "guid",
      cellRenderer: OverviewActionsCell,
      cellRendererParams: (params: any) => ({
        instanceId: this.instanceId,
        params
      }),
      maxWidth: 160
    }
  ]

  constructor(private route: ActivatedRoute) {
    this.route.params.subscribe(params => {
      this.instanceId = params['id'];
      InstanceService.getApiInstanceGetServerInformation(this.instanceId).then(serverInformation => {
        this.serverInformation.set(serverInformation);
      });
    });
  }

  ngOnInit() {
    this.timer = setInterval(() => {
      InstanceService.getApiInstanceGetServerInformation(this.instanceId).then(serverInformation => {
        this.serverInformation.set(serverInformation);
      });
    }, 5000);
  }

  ngOnDestroy() {
    clearInterval(this.timer);
  }

  public onStartClick(): void {
    InstanceService.getApiInstanceStartServer(this.instanceId).then();
  }

  public onStopClick(): void {
    InstanceService.getApiInstanceStopServer(this.instanceId).then();
  }

  public onRemoveClicked(){
    InstanceService.deleteApiInstanceRemoveServer(this.instanceId).then(() => {
      this.router.navigate(['/']);
    });
  }
}
