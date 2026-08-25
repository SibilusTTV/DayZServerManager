import {Component} from '@angular/core';
import {ICellRendererAngularComp} from 'ag-grid-angular';
import {ICellRendererParams} from 'ag-grid-community';
import {MatFormField, MatInput, MatLabel} from '@angular/material/input';
import {ReactiveFormsModule} from '@angular/forms';

@Component({
  selector: 'timespan-cell-renderer',
  templateUrl: './timespan-cell-renderer.html',
  imports: [
    MatFormField,
    MatInput,
    ReactiveFormsModule
  ]
})
export default class TimespanCellRenderer implements ICellRendererAngularComp{
  public timespan: string = "";
  public params: any;
  public id: number = 0;

  agInit(params: ICellRendererParams) {
    this.params = params;
    this.timespan = params.value;
    this.id = params.data.id;
  }

  refresh(params: ICellRendererParams) {
    this.params = params;
    this.timespan = params.value;
    this.id = params.data.id;
    return true;
  }

  onChange(event: any) {
    this.timespan = event.target?.value;
    this.params.change(this.id, this.timespan);
  }
}
