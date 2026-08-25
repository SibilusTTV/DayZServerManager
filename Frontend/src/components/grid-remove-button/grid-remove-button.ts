import {Component} from '@angular/core';
import {MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {ICellRendererAngularComp} from 'ag-grid-angular';
import {ICellRendererParams} from 'ag-grid-community';

@Component({
  selector: 'grid-remove-button',
  templateUrl: './grid-remove-button.html',
  imports: [
    MatIconButton,
    MatIcon
  ]
})
export default class GridRemoveButton implements ICellRendererAngularComp{
  public id: number = 0;
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

  public onRemoveClicked(){
    this.params.remove(this.id);
  }
}
