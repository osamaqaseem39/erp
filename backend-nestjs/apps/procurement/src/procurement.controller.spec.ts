import { Test, TestingModule } from '@nestjs/testing';
import { ProcurementController } from './procurement.controller';
import { ProcurementService } from './procurement.service';

describe('ProcurementController', () => {
  let procurementController: ProcurementController;

  beforeEach(async () => {
    const app: TestingModule = await Test.createTestingModule({
      controllers: [ProcurementController],
      providers: [ProcurementService],
    }).compile();

    procurementController = app.get<ProcurementController>(ProcurementController);
  });

  describe('root', () => {
    it('should return "Hello World!"', () => {
      expect(procurementController.getHello()).toBe('Hello World!');
    });
  });
});
