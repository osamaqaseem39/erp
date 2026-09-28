import { Controller, Get } from '@nestjs/common';
import { ProcurementService } from './procurement.service';

@Controller()
export class ProcurementController {
  constructor(private readonly procurementService: ProcurementService) {}

  @Get()
  getHello(): string {
    return this.procurementService.getHello();
  }
}
