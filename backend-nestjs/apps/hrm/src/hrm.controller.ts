import { Controller, Get } from '@nestjs/common';
import { HrmService } from './hrm.service';

@Controller()
export class HrmController {
  constructor(private readonly hrmService: HrmService) {}

  @Get()
  getHello(): string {
    return this.hrmService.getHello();
  }
}
