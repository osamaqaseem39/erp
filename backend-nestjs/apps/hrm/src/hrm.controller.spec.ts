import { Test, TestingModule } from '@nestjs/testing';
import { HrmController } from './hrm.controller';
import { HrmService } from './hrm.service';

describe('HrmController', () => {
  let hrmController: HrmController;

  beforeEach(async () => {
    const app: TestingModule = await Test.createTestingModule({
      controllers: [HrmController],
      providers: [HrmService],
    }).compile();

    hrmController = app.get<HrmController>(HrmController);
  });

  describe('root', () => {
    it('should return "Hello World!"', () => {
      expect(hrmController.getHello()).toBe('Hello World!');
    });
  });
});
