import { Module } from '@nestjs/common';
import { ProcurementController } from './procurement.controller';
import { ProcurementService } from './procurement.service';

@Module({
  imports: [],
  controllers: [ProcurementController],
  providers: [ProcurementService],
})
export class ProcurementModule {}
