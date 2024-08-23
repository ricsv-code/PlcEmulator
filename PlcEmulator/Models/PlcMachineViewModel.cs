using System.Windows.Controls.Primitives;

namespace PlcEmulator
{
    public class PlcMachineViewModel : ViewModelBase
    {

        public PlcMachineViewModel(int numberOfMotors)
        {
            Motors = new List<MotorViewModel>();
            _machine = new PlcMachine();

            for (int _ = 0; _ < numberOfMotors; _++)
            {
                Motors.Add(new MotorViewModel());
            }
        }

        private PlcMachine _machine;

        public PlcMachine Machine => _machine;

        public List<MotorViewModel> Motors { get; set; }

        public bool OverrideKey 
        { 
            get => _machine.OverrideKey; 
            set => SetProperty(ref _machine.OverrideKey, value); 
        }
        public bool OperationMode
        {
            get => _machine.OperationMode;
            set => SetProperty(ref _machine.OperationMode, value);
        }
        public bool MachineNeedsHoming
        {
            get => _machine.MachineNeedsHoming;
            set => SetProperty(ref _machine.MachineNeedsHoming, value);
        }
        public bool MachineInCenter => Motors.All(motor => motor.InCenteredPosition);

        public bool MachineStill => !Motors.Any(motor => motor.MotorInProgress);

        public bool MachineInMotion => Motors.Any(motor => motor.MotorInProgress);

        public bool MachineInHome => Motors.All(motor => motor.InHomePosition);

        //Status byte 5 on code 255
        public bool ProhibitMovement
        {
            get => _machine.ProhibitMovement;
            set => SetProperty(ref _machine.ProhibitMovement, value);
        }
        public bool SickReset
        {
            get => _machine.SickReset;
            set => SetProperty(ref _machine.SickReset, value);
        }
        public bool SickActive
        {
            get => _machine.SickActive;
            set => SetProperty(ref _machine.SickActive, value);
        }
        public bool EStopReset
        {
            get => _machine.EStopReset;
            set => SetProperty(ref _machine.EStopReset, value);
        }
        public bool EStop
        {
            get => _machine.EStop;
            set => SetProperty(ref _machine.EStop, value);
        }

        public UniformGrid MotorGrid
        {
            get => _machine.MotorGrid;
            set => SetProperty(ref _machine.MotorGrid, value);
        }

    }
}


