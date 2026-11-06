export interface VehicleInput {
  plate: string;
  manufactureYear: number;
  brand: string;
  model: string;
  personId: number;
}

export interface Vehicle extends VehicleInput {
  id: number;
}
