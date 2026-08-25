import {Component, Input, input, signal, WritableSignal} from '@angular/core';
import {CustomMessage, Mod} from '../api';
import {ColDef} from 'ag-grid-community';
import {v4} from 'uuid';
import {AgGridAngular} from 'ag-grid-angular';
import {MatFormField, MatHint, MatInput, MatLabel} from '@angular/material/input';
import {MatOption, MatSelect} from '@angular/material/select';
import {FormsModule} from '@angular/forms';
import {MatIconButton} from '@angular/material/button';
import GridRemoveButton from './grid-remove-button/grid-remove-button';
import TimespanCellRenderer from './timespan-cell-renderer/timespan-cell-renderer';
import {MatIcon} from '@angular/material/icon';

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
  @Input() customMessages: WritableSignal<CustomMessage[]> = signal([]);

  @Input() id: WritableSignal<string> = signal("");
  @Input() instanceId: WritableSignal<number> = signal(0);
  @Input() serverFolder: WritableSignal<string> = signal("");
  @Input() hostName: WritableSignal<string> = signal("");
  @Input() missionName: WritableSignal<string> = signal("");
  @Input() vanillaMissionName: WritableSignal<string> = signal("");
  @Input() missionTemplateName: WritableSignal<string> = signal("");
  @Input() serverConfigName: WritableSignal<string> = signal("");
  @Input() profileName: WritableSignal<string> = signal("");
  @Input() schedulerConfigName: WritableSignal<string> = signal("");
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
  @Input() restartOnUpdate: WritableSignal<boolean> = signal(false);
  @Input() restartInterval: WritableSignal<number> = signal(0);
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
      cellEditor: "agNumberCellEditor"
    },
    {
      headerName: "Remove",
      field: "id",
      cellRenderer: GridRemoveButton,
      cellRendererParams: {
        remove: this.onClientGridRemove.bind(this)
      }
    }
  ];

  public serverColDefs: ColDef[] = [
    {
      field: "name",
      rowDrag: true,
    },
    {
      field: "workshopID",
      cellEditor: "agNumberCellEditor"
    },
    {
      headerName: "Remove",
      field: "id",
      cellRenderer: GridRemoveButton,
      cellRendererParams: {
        remove: this.onServerGridRemove.bind(this)
      }
    }
  ];

  public messageColDefs: ColDef[] = [
    {
      field: "isTimeOfDay",
      cellEditor: "agCheckboxCellEditor",
      width: 140,
      rowDrag: true
    },
    {
      field: "waitTime",
      headerName: "Wait Time / Time Of Day",
      editable: false,
      cellRenderer: TimespanCellRenderer,
      cellRendererParams: (params: any) => ({
        id: params.id,
        timespan: params.waitTime,
        change: this.onMessageWaitTimeChange.bind(this)
      }),
      width: 280
    },
    {
      field: "interval",
      editable: false,
      cellRenderer: TimespanCellRenderer,
      cellRendererParams: (params: any) => ({
        id: params.id,
        timespan: params.waitTime,
        change: this.onMessageIntervalChange.bind(this)
      }),
      width: 280
    },
    {
      field: "title",
      width: 120
    },
    {
      field: "message"
    },
    {
      field: "icon",
      width: 160
    },
    {
      field: "color",
      width: 120
    },
    {
      headerName: "Remove",
      field: "id",
      cellRenderer: GridRemoveButton,
      cellRendererParams: {
        remove: this.onMessageGridRemove.bind(this)
      },
      width: 100
    }
  ]

  public defaultColDef: ColDef = {
    editable: true
  }

  constructor() {

  }

  public onAddClientModClick(){
    this.clientMods.set([
      ...this.clientMods(),
      {
        id: v4(),
        name: "",
        workshopID: 0
      }
    ])
  }

  public onAddServerModClick(){
    this.serverMods.set([
      ...this.serverMods(),
      {
        id: v4(),
        name: "",
        workshopID: 0
      }
    ])
  }

  public onAddCustomMessageClick(){
    this.customMessages.set([
      ...this.customMessages(),
      {
        id: v4(),
        isTimeOfDay: false,
        waitTime: "00:00:00",
        interval: "00:00:00",
        title: "",
        message: "",
        icon: "",
        color: ""
      }
    ])
  }

  onClientRowDragEnd(params: any) {
    const newData: Mod[] = [];
    params.api.forEachNode((node: any) => {
      newData.push(node.data);
    });

    this.clientMods.set([...newData]);
  }

  onServerRowDragEnd(params: any) {
    const newData: Mod[] = [];
    params.api.forEachNode((node: any) => {
      newData.push(node.data);
    });

    this.serverMods.set([...newData]);
  }

  onMessageRowDragEnd(params: any) {
    const newData: CustomMessage[] = [];
    params.api.forEachNode((node: any) => {
      newData.push(node.data);
    });

    this.customMessages.set([...newData]);
  }

  onClientGridRemove(id: string) {
    const newData: Mod[] = this.clientMods().filter(mod => mod.id != id);
    this.clientMods.set([...newData]);
  }

  onServerGridRemove(id: string) {
    const newData: Mod[] = this.serverMods().filter(mod => mod.id != id);
    this.serverMods.set([...newData]);
  }

  onMessageGridRemove(id: string) {
    const newData: CustomMessage[] = this.customMessages().filter(mod => mod.id != id);
    this.customMessages.set([...newData]);
  }

  onMessageWaitTimeChange(id: string, waitTime: string) {
    const newData: CustomMessage[] = this.customMessages().map(message => {
      if (message.id == id){
        return {
          ...message,
          waitTime: waitTime
        }
      }
      return message;
    });
    this.customMessages.set([...newData]);
  }

  onMessageIntervalChange(id: string, interval: string) {
    const newData: CustomMessage[] = this.customMessages().map(message => {
      if (message.id == id){
        return {
          ...message,
          interval: interval
        }
      }
      return message;
    });
    this.customMessages.set([...newData]);
  }
}
