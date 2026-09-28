import { Module } from '@nestjs/common';
import { ManufacturingController } from './manufacturing.controller';
import { ManufacturingService } from './manufacturing.service';

@Module({
  imports: [],
  controllers: [ManufacturingController],
  providers: [ManufacturingService],
})
export class ManufacturingModule {}
