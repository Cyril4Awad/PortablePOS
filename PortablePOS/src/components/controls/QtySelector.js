import React, { useMemo } from 'react';
import { View, Text, TextInput, Pressable } from 'react-native';

function clamp(n, min, max) {
  return Math.min(Math.max(n, min), max);
}

export function QtySelector({
  value = 1,
  min = 1,
  max = 9999,
  onChange = () => {},
}) {
  const decDisabled = value <= min;
  const incDisabled = value >= max;

  const handleDec = () => {
    if (!decDisabled) onChange(clamp(value - 1, min, max));
  };
  const handleInc = () => {
    if (!incDisabled) onChange(clamp(value + 1, min, max));
  };

  const handleChangeText = (t) => {
    // Allow temporary empty string while typing
    if (t === '') {
      onChange(min); // keep >= min
      return;
    }
    const n = parseInt(t.replace(/[^\d]/g, ''), 10);
    if (isNaN(n)) {
      onChange(min);
    } else {
      onChange(clamp(n, min, max));
    }
  };

  const handleBlur = () => {
    // Snap back to min if somehow < min
    if (value < min) onChange(min);
  };

  return (
    <View style={qtyStyles.wrapper}>

  
<Pressable
        onPress={handleInc}
        disabled={incDisabled}
        android_ripple={{ borderless: true }}
        style={({ pressed }) => [
          qtyStyles.iconBtn,
          incDisabled && qtyStyles.btnDisabled,
          pressed && qtyStyles.btnPressed,
        ]}
        accessibilityRole="button"
        accessibilityLabel="Increase quantity"
      >
        <Text style={[qtyStyles.iconText, incDisabled && qtyStyles.iconDisabled]}>+</Text>
      </Pressable>

      <TextInput
        value={String(value)}
        onChangeText={handleChangeText}
        onBlur={handleBlur}
        keyboardType="number-pad"
        returnKeyType="done"
        style={qtyStyles.input}
        maxLength={5}
        accessibilityLabel="Quantity"
      />


      <Pressable
        onPress={handleDec}
        disabled={decDisabled}
        android_ripple={{ borderless: true }}
        style={({ pressed }) => [
          qtyStyles.iconBtn,
          decDisabled && qtyStyles.btnDisabled,
          pressed && qtyStyles.btnPressed,
        ]}
        accessibilityRole="button"
        accessibilityLabel="Decrease quantity"
      >
        <Text style={[qtyStyles.iconText, decDisabled && qtyStyles.iconDisabled]}>−</Text>
      </Pressable>

    </View>
  );
}

const qtyStyles = {
  wrapper: {
    flexDirection: 'column',
    alignItems: 'center',
    borderRadius: 14,
    padding: 6,
    //backgroundColor: '#fff',
    //shadowColor: '#000',
    //shadowOpacity: 0.08,
    //shadowRadius: 8,
    //shadowOffset: { width: 0, height: 2 },
    //elevation: 3,
  },
  iconBtn: {
    width: 10,
    height: 10,
    borderRadius: 20,
    alignItems: 'center',
    justifyContent: 'center',
    //backgroundColor: '#F2F3F5',
  },
  btnPressed: { opacity: 0.8 },
  //btnDisabled: { backgroundColor: '#EEE' },
  iconText: { fontSize: 10, fontWeight: '700' },
  iconDisabled: { opacity: 0.35 },
  input: {
    minWidth: 16,
    textAlign: 'center',
    fontSize: 10,
    fontWeight: '600',
    marginHorizontal: 8,
    paddingVertical: 6,
    paddingHorizontal: 8,
    borderRadius: 10,
    //backgroundColor: '#FAFAFA',
  },
};
