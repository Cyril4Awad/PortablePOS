import { FormatQuantity } from './PhysicalCountRules';

// Free Receiving rules : the receiving orders are prepared on the Receiving Order screens (ReceivingOrderRules),
// quantities of the totals as whole thousandths like the physical count (12.5 => 12500)

// Rows of one download page
export const PAGE_SIZE = 5000;
export const PICK_LIMIT = 200;
export const LIST_LIMIT = 500;

export { FormatQuantity };
