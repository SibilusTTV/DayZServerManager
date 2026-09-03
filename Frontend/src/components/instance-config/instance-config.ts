import {Component, Input, input, signal, WritableSignal} from '@angular/core';
import {CustomMessage, Mod} from '../../api';
import {type AutoSizeStrategy, ColDef} from 'ag-grid-community';
import {v4} from 'uuid';
import {AgGridAngular} from 'ag-grid-angular';
import {MatFormField, MatHint, MatInput, MatLabel} from '@angular/material/input';
import {MatOption, MatSelect} from '@angular/material/select';
import {FormsModule} from '@angular/forms';
import {MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import GridRemoveButton from '../grid-remove-button/grid-remove-button';

@Component({
  selector: 'instance-config',
  templateUrl: './instance-config.html',

  imports: [
    AgGridAngular,
    MatFormField,
    MatLabel,
    MatSelect,
    MatOption,
    FormsModule,
    MatInput,
    MatHint,
    MatIconButton,
    MatIcon
  ]
})
export default class InstanceConfig {

  @Input() clientMods: WritableSignal<Mod[]> = signal([]);
  @Input() serverMods: WritableSignal<Mod[]> = signal([]);

  @Input() id: WritableSignal<number> = signal(0);
  @Input() serverFolder: WritableSignal<string> = signal("");
  @Input() hostName: WritableSignal<string> = signal("");
  @Input() missionName: WritableSignal<string> = signal("");
  @Input() vanillaMissionName: WritableSignal<string> = signal("");
  @Input() missionTemplateName: WritableSignal<string> = signal("");
  @Input() serverConfigName: WritableSignal<string> = signal("");
  @Input() profileName: WritableSignal<string> = signal("");
  @Input() steamPort: WritableSignal<number> = signal(0);
  @Input() serverPort: WritableSignal<number> = signal(0);
  @Input() steamQueryPort: WritableSignal<number> = signal(0);
  @Input() rConPort: WritableSignal<number> = signal(0);
  @Input() rConPassword: WritableSignal<string> = signal("");
  @Input() cpuCount: WritableSignal<number> = signal(0);
  @Input() noFilePatching: WritableSignal<boolean> = signal(false);
  @Input() doLogs: WritableSignal<boolean> = signal(false);
  @Input() adminLog: WritableSignal<boolean> = signal(false);
  @Input() freezeCheck: WritableSignal<boolean> = signal(false);
  @Input() netLog: WritableSignal<boolean> = signal(false);
  @Input() limitFPS: WritableSignal<number> = signal(0);
  @Input() mapName: WritableSignal<string> = signal("");
  @Input() autoStartServer: WritableSignal<boolean> = signal(false);
  @Input() makeBackups: WritableSignal<boolean> = signal(false);
  @Input() deleteBackups: WritableSignal<boolean> = signal(false);
  @Input() backupPath: WritableSignal<string> = signal("");
  @Input() maxKeepTime: WritableSignal<number> = signal(0);

  public clientColDefs: ColDef[] = [
    {
      field: "name",
      rowDrag: true
    },
    {
      field: "workshopID",
      cellEditor: "agNumberCellEditor",
      resizable: false,
      maxWidth: 160
    },
    {
      headerName: "Actions",
      field: "id",
      cellRenderer: GridRemoveButton,
      cellRendererParams: {
        remove: this.onClientGridRemove.bind(this)
      },
      resizable: false,
      maxWidth: 100
    }
  ];

  public serverColDefs: ColDef[] = [
    {
      field: "name",
      rowDrag: true,
    },
    {
      field: "workshopID",
      cellEditor: "agNumberCellEditor",
      resizable: false,
      maxWidth: 160
    },
    {
      headerName: "Actions",
      field: "id",
      cellRenderer: GridRemoveButton,
      cellRendererParams: {
        remove: this.onServerGridRemove.bind(this)
      },
      resizable: false,
      maxWidth: 100
    }
  ];

  public defaultColDef: ColDef = {
    editable: true
  }

  public autoSizeStrategy: AutoSizeStrategy ={
    type: "fitGridWidth"
  }

  constructor() {

  }

  public onAddClientModClick(){
    this.clientMods.set([
      ...this.clientMods(),
      {
        id: v4().toLowerCase(),
        name: "",
        workshopID: 0
      }
    ])
  }

  public onAddServerModClick(){
    this.serverMods.set([
      ...this.serverMods(),
      {
        id: v4().toLowerCase(),
        name: "",
        workshopID: 0
      }
    ])
  }

  onClientRowDragStopped(params: any) {
    const newData: Mod[] = [];
    params.api.forEachNode((node: any) => {
      newData.push(node.data);
    });

    this.clientMods.set([...newData]);
  }

  onServerRowDragStopped(params: any) {
    const newData: Mod[] = [];
    params.api.forEachNode((node: any) => {
      newData.push(node.data);
    });

    this.serverMods.set([...newData]);
  }

  onClientGridRemove(id: string) {
    const newData: Mod[] = this.clientMods().filter(mod => mod.id != id);
    this.clientMods.set([...newData]);
  }

  onServerGridRemove(id: string) {
    const newData: Mod[] = this.serverMods().filter(mod => mod.id != id);
    this.serverMods.set([...newData]);
  }
}
