export type ScannerMode = 'camera' | 'photo';

export type ScannerDraft = {
  barcode: string;
  imageUri?: string | null;
  name: string;
};
