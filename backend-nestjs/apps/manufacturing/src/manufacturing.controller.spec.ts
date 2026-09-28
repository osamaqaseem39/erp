import { Test, TestingModule } from '@nestjs/testing';
import { ManufacturingController } from './manufacturing.controller';
import { ManufacturingService } from './manufacturing.service';

describe('ManufacturingController', () => {
  let manufacturingController: ManufacturingController;

  beforeEach(async () => {
    const app: TestingModule = await Test.createTestingModule({
      controllers: [ManufacturingController],
      providers: [ManufacturingService],
    }).compile();

    manufacturingController = app.get<ManufacturingController>(ManufacturingController);
  });

  describe('root', () => {
    it('should return "Hello World!"', () => {
      expect(manufacturingController.getHello()).toBe('Hello World!');
    });
  });
});
