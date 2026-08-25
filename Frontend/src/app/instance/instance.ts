import {Component} from '@angular/core';
import {ActivatedRoute, RouterOutlet} from '@angular/router';
import {MatToolbar} from '@angular/material/toolbar';
import {MatButton} from '@angular/material/button';
import {MatMenu, MatMenuItem, MatMenuTrigger} from '@angular/material/menu';

@Component({
  selector: 'instance-root',
  imports: [
    MatToolbar,
    MatButton,
    MatMenuTrigger,
    MatMenu,
    MatMenuItem,
    RouterOutlet
  ],
  templateUrl: './instance.html'
})
export default class Instance {
  public id: number;

  constructor(private route: ActivatedRoute) {
    this.id = 0;
    this.route.params.subscribe(params => {
      this.id = params['id'];
    });
  }

  public getOverviewRoute(){
    return "instance/" + this.id + "/overview";
  }

  public getInstanceConfigEditorRoute(){
    return "instance/" + this.id + "/instance-config-editor";
  }

  public getServerConfigEditorRoute(){
    return "instance/" + this.id + "/server-config-editor";
  }

  public getVanillaRaritiesRoute(){
    return "instance/" + this.id + "/vanilla-rarities";
  }

  public getCustomFilesRaritiesRoute(){
    return "instance/" + this.id + "/custom-files-rarities";
  }

  public getExpansionRaritiesRoute(){
    return "instance/" + this.id + "/expansion-rarities";
  }
}
