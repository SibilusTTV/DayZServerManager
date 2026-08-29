import {Component, signal, WritableSignal} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {PropertyValue, ServerConfigService} from '../../../../api';
import {AgGridAngular} from 'ag-grid-angular';
import {type AutoSizeStrategy, ColDef, EditableCallbackParams} from 'ag-grid-community';
import {MatIcon} from '@angular/material/icon';
import {MatIconButton} from '@angular/material/button';
import ServerConfigActionsCell from './server-config-actions-cell/server-config-actions-cell';


@Component({
  selector: 'server-config-editor',
  templateUrl: './server-config-editor.html',
  imports: [
    AgGridAngular,
    MatIcon,
    MatIconButton
  ]
})
export default class ServerConfigEditor {
  private id: string = "";
  public properties: WritableSignal<PropertyValue[]> = signal([]);

  public ColDefs: ColDef[] = [
    {
      field: "propertyName",
      maxWidth: 320,
      editable: params => this.isEditable(params),
      filter: true
    },
    {
      field: "value",
      maxWidth: 320,
      editable: params => this.isEditable(params),
      filter: true
    },
    {
      field: "comment",
      editable: true,
      filter: true
    },
    {
      cellRenderer: ServerConfigActionsCell,
      cellRendererParams: (params: any) => ({
        onDeleteClick: this.onDeleteClick.bind(this),
        params
      }),
      maxWidth: 80
    }
  ];

  public DefaultColDef: ColDef = {
    editable: false
  }

  public autoSizeStrategy: AutoSizeStrategy ={
    type: "fitGridWidth",
    defaultMinWidth: 80
  }

  constructor(private route: ActivatedRoute) {
    this.id = "";
    this.route.params.subscribe(params => {
      this.id = params['id'];
      this.LoadServerConfig();
    });
  }

  public isEditable(params: EditableCallbackParams){
    return params.data.propertyName != "hostname"
      && params.data.propertyName != "steamPort"
      && params.data.propertyName != "steamQueryPort"
      && params.data.propertyName != "instanceId"
      && params.data.propertyName != "template";
  }

  public LoadServerConfig(){
    ServerConfigService.getApiServerConfigGet(this.id).then((response) => {
      this.properties.set(response);
    })
  }

  public onSaveClick(){
    ServerConfigService.postApiServerConfigPost( this.id, this.properties()).then(() => {
      this.LoadServerConfig();
    })
  }

  public onAddPropertyClick(){
    this.properties.set([...this.properties(), {
      propertyName: "propertyName",
      value: "",
      comment: ""
    }]);
  }

  public onDeleteClick(propertyName: string, value: string, comment: string){
    this.properties.set(this.properties().filter(x => !(x.value == value && x.propertyName == propertyName && x.comment == comment)));
  }
}
