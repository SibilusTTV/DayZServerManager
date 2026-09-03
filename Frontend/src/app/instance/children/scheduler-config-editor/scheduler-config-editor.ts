import {Component, Input, signal, WritableSignal} from '@angular/core';
import {MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {ActivatedRoute} from '@angular/router';
import {CustomMessage, InstanceService, Mod, SchedulerConfig, SchedulerService} from '../../../../api';
import {type AutoSizeStrategy, ColDef} from 'ag-grid-community';
import {v4} from 'uuid';
import {AgGridAngular} from 'ag-grid-angular';
import {FormsModule} from '@angular/forms';
import {MatFormField, MatHint, MatInput, MatLabel} from '@angular/material/input';
import {MatOption} from '@angular/material/core';
import {MatSelect} from '@angular/material/select';
import GridRemoveButton from '../../../../components/grid-remove-button/grid-remove-button';
import TimespanCellRenderer from '../../../../components/timespan-cell-renderer/timespan-cell-renderer';
import {FlatGridRenderer} from '../../../../components/flat-grid-renderer/flat-grid-renderer';

@Component({
  selector: 'scheduler-config-editor',
  templateUrl: './scheduler-config-editor.html',
  imports: [
    MatIconButton,
    MatIcon,
    AgGridAngular,
    FormsModule,
    MatFormField,
    MatHint,
    MatInput,
    MatLabel,
    MatOption,
    MatSelect
  ]
})
export class SchedulerConfigEditor {
  public messageColDefs: ColDef[] = [
    {
      field: "isTimeOfDay",
      cellEditor: "agCheckboxCellEditor",
      resizable: false,
      maxWidth: 140,
      rowDrag: true
    },
    {
      field: "waitTime",
      headerName: "Wait Time / Time Of Day",
      editable: false,
      cellRenderer: TimespanCellRenderer,
      cellRendererParams: (params: any) => ({
        id: params.instanceId,
        timespan: params.waitTime,
        change: this.onMessageWaitTimeChange.bind(this)
      }),
      resizable: false,
      maxWidth: 200
    },
    {
      field: "interval",
      editable: false,
      cellRenderer: TimespanCellRenderer,
      cellRendererParams: (params: any) => ({
        id: params.instanceId,
        timespan: params.waitTime,
        change: this.onMessageIntervalChange.bind(this)
      }),
      resizable: false,
      maxWidth: 140
    },
    {
      field: "title"
    },
    {
      field: "message"
    },
    {
      field: "icon"
    },
    {
      field: "color"
    },
    {
      headerName: "Actions",
      field: "id",
      cellRenderer: GridRemoveButton,
      cellRendererParams: {
        remove: this.onMessageGridRemove.bind(this)
      },
      resizable: false,
      maxWidth: 100
    }
  ];

  public badNamesColDefs: ColDef[] = [
    {
      headerName: "Name",
      cellRenderer: FlatGridRenderer
    },
    {
      headerName: "Actions",
      cellRenderer: GridRemoveButton,
      cellRendererParams: (params: any) => ({
        remove: this.onBadNameRemove.bind(this),
        params
      }),
      maxWidth: 100
    }
  ]

  public defaultColDef: ColDef = {
    editable: true
  }

  public autoSizeStrategy: AutoSizeStrategy ={
    type: "fitGridWidth"
  }

  private instanceId: number = 0;
  private id: string = "";
  public restartOnUpdate: WritableSignal<boolean> = signal(false);
  public restartInterval: WritableSignal<number> = signal(0);
  public customMessages: WritableSignal<CustomMessage[]> = signal([]);
  public badNames: WritableSignal<string[]> = signal([]);
  public timeout: WritableSignal<number> = signal(0);
  public useNickFilter: WritableSignal<boolean> = signal(false);
  public filteredNickMessage: WritableSignal<string> = signal("");
  public badName: WritableSignal<string> = signal("");

  constructor(private route: ActivatedRoute) {
    this.instanceId = 0;
    this.route.params.subscribe(params => {
      this.instanceId = params['id'];
      this.LoadServerConfig();
    });
  }

  public onSaveClick(){
    const schedulerConfig: SchedulerConfig = {
      id: this.id,
      instanceId: this.instanceId,
      badNames: this.badNames(),
      filteredNickMsg: this.filteredNickMessage(),
      useNickFilter: this.useNickFilter(),
      timeout: this.timeout(),
      restartOnUpdate: this.restartOnUpdate(),
      restartInterval: this.restartInterval(),
      customMessages: this.customMessages().map((message, index) => {
        return {
          ...message,
          position: index
        }
      })
    }

    SchedulerService.postApiSchedulerCreateEditSchedulerConfig(schedulerConfig).then();
  }

  public LoadServerConfig(){
    SchedulerService.getApiSchedulerGetSchedulerConfig(this.instanceId).then(response => {
      this.id = response?.id ?? v4().toLowerCase();
      this.badNames.set(response?.badNames ?? []);
      this.filteredNickMessage.set(response?.filteredNickMsg ?? "");
      this.timeout.set(response?.timeout ?? 0);
      this.useNickFilter.set(response?.useNickFilter ?? false);
      this.restartOnUpdate.set(response?.restartOnUpdate ?? false);
      this.restartInterval.set(response?.restartInterval ?? 1);
      this.customMessages.set(response?.customMessages ?? []);
    })
  }

  public onAddCustomMessageClick(){
    this.customMessages.set([
      ...this.customMessages(),
      {
        id: v4().toLowerCase(),
        position: 0,
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

  onMessageRowDragStopped(params: any) {
    const newData: CustomMessage[] = [];
    params.api.forEachNode((node: any) => {
      newData.push(node.data);
    });

    this.customMessages.set([...newData]);
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

  onBadNameRemove(name: string){
    const newData: string[] = this.badNames().filter(str => str == name);
    this.badNames.set([...newData]);
  }

  onAddBadName(){
    const newData: string[] = [...this.badNames(), this.badName()];
    this.badNames.set([...newData]);
    this.badName.set("");
  }
}
