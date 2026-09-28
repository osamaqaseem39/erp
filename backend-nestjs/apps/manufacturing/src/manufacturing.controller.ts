import { Controller, Get } from '@nestjs/common';
import { ManufacturingService } from './manufacturing.service';

@Controller()
export class ManufacturingController {
  constructor(private readonly manufacturingService: ManufacturingService) {}

  @Get()
  getHello(): string {
    return this.manufacturingService.getHello();
  }
}
