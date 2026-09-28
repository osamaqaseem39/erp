import { Controller, Get } from '@nestjs/common';
import { ReportsService } from './reports.service';

@Controller()
export class ReportsController {
  constructor(private readonly reportsService: ReportsService) {}

  @Get()
  getHello(): string {
    return this.reportsService.getHello();
  }
}
