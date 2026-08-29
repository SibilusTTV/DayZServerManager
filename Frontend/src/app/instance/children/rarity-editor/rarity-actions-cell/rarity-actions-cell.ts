import {Component} from '@angular/core';
import {ICellRendererAngularComp} from 'ag-grid-angular';
import {ICellRendererParams} from 'ag-grid-community';
import {MatIconButton} from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';

@Component({
  selector: 'rarity-actions-cell',
  templateUrl: './rarity-actions-cell.html',
  imports: [
    MatIconButton,
    MatIcon
  ]
})
export class RarityActionsCell implements ICellRendererAngularComp {
  private params: any;
  private id: number = -1;

  agInit(params: ICellRendererParams) {
    this.params = params;
    this.id = this.params.value;
  }

  refresh(params: ICellRendererParams) {
    this.params = params;
    this.id = this.params.value;
    return true;
  }

  public onDeleteClick(): void {
    this.params.onDeleteClick(this.id);
  }
}
