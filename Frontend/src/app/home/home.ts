import {Component, inject, signal, WritableSignal} from '@angular/core';
import { AgGridAngular } from 'ag-grid-angular';
import type {AutoSizeStrategy, ColDef} from 'ag-grid-community';
import {RowDoubleClickedEvent} from 'ag-grid-community';
import {Router} from '@angular/router';
import {MatFormField, MatInput, MatLabel} from '@angular/material/input';
import {MatButton, MatIconButton} from '@angular/material/button';
import {FormsModule} from '@angular/forms';
import ServerControlCell from './server-control-cell/server-control-cell';
import {ServerInformationCell} from './server-information-cell/server-information-cell';
import {MatDialog} from '@angular/material/dialog';
import {NewInstanceDialog} from './new-instance-dialog/new-instance-dialog';
import {MatIcon} from '@angular/material/icon';
import {Instance, InstanceService, SteamCredentials, SteamService} from '../../api';
import {v4} from 'uuid';

@Component({
  selector: 'app-home',
  imports: [
    AgGridAngular,
    MatFormField,
    MatLabel,
    MatInput,
    MatButton,
    FormsModule,
    MatIcon,
    MatIconButton
  ],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export default class Home {
  public instances: WritableSignal<Instance[]> = signal([]);
  private router = inject(Router);
  public steamUsername: WritableSignal<string> = signal("");
  public steamPassword: WritableSignal<string> = signal("");
  private steamCredentialsId: string = "";
  private readonly dialog = inject(MatDialog);

  public colDefs: ColDef[] = [
    {
      field: "id",
      filter: true
    },
    {
      field: "hostName",
      filter: true
    },
    {
      field: "serverFolder",
      filter: true
    },
    {
      field: "id",
      headerName: "Server Status",
      cellRenderer: ServerInformationCell
    },
    {
      field: "id",
      headerName: "Functions",
      cellRenderer: ServerControlCell,
      cellRendererParams: {
        reloadManager: this.getManagerConfig.bind(this)
      }
    }
  ];

  public autoSizeStrategy: AutoSizeStrategy ={
    type: "fitGridWidth"
  }

  constructor() {
    this.getManagerConfig();
  }

  public getManagerConfig() {
    InstanceService.getApiInstanceGetInstances().then(response => {
      this.instances.set(response);
    });
    SteamService.getApiSteamGetSteamCredentials().then(response => {
      this.steamCredentialsId = response.id ?? v4();
      this.steamUsername.set(response.steamUsername ?? "");
      this.steamPassword.set(response.steamPassword ?? "");
    });
  }

  public onRowDoubleClick(event: RowDoubleClickedEvent<Instance>){
    if (event.data?.id != null){
      this.router.navigate(["/instance", event.data?.id, "overview"])
    }
  }

  public onSaveClicked(){
    const credentials: SteamCredentials = {
      id: this.steamCredentialsId,
      steamUsername: this.steamUsername(),
      steamPassword: this.steamPassword()
    }
    SteamService.postApiSteamSaveSteamCredentials(credentials).then();
  }

  public onNewInstanceClicked(){
    const dialogRef = this.dialog.open(NewInstanceDialog, {
      minWidth: '800px',
      maxWidth: '1440px',
    });

    dialogRef.afterClosed().subscribe(result => {
      this.getManagerConfig();
    });
  }
}
