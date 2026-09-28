import { Test, TestingModule } from '@nestjs/testing';
import { ReportsController } from './reports.controller';
import { ReportsService } from './reports.service';

describe('ReportsController', () => {
  let reportsController: ReportsController;

  beforeEach(async () => {
    const app: TestingModule = await Test.createTestingModule({
      controllers: [ReportsController],
      providers: [ReportsService],
    }).compile();

    reportsController = app.get<ReportsController>(ReportsController);
  });

  describe('root', () => {
    it('should return "Hello World!"', () => {
      expect(reportsController.getHello()).toBe('Hello World!');
    });
  });
});
