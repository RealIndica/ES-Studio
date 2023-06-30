#ifndef UNITS_H
#define UNITS_H

namespace constants {

    constexpr double pi = 3.14159265359;
    constexpr double R = 8.31446261815324;
    constexpr double root_2 = 1.41421356237309504880168872420969807856967187537694807317667973799;
    constexpr double e = 2.71828182845904523536028747135266249775724709369995;

}

namespace units {
    // Force
    constexpr double N = 1.0;

    constexpr double lbf = N * 4.44822;

    // Mass
    constexpr double kg = 1.0;
    constexpr double g = kg / 1000.0;

    constexpr double lb = 0.45359237 * kg;

    // Distance
    constexpr double m = 1.0;
    constexpr double cm = m / 100.0;
    constexpr double mm = m / 1000.0;
    constexpr double km = m * 1000.0;

    constexpr double inch = cm * 2.54;
    constexpr double foot = inch * 12.0;
    constexpr double thou = inch / 1000.0;
    constexpr double mile = m * 1609.344;

    // Time
    constexpr double sec = 1.0;
    constexpr double minute = 60 * sec;
    constexpr double hour = 60 * minute;

    // Torque
    constexpr double Nm = N * m;
    constexpr double ft_lb = foot * lbf;

    // Power
    constexpr double W = Nm / sec;
    constexpr double kW = W * 1000.0;
    constexpr double hp = 745.699872 * W;

    // Volume
    constexpr double m3 = 1.0;
    constexpr double cc = cm * cm * cm;
    constexpr double mL = cc;
    constexpr double L = mL * 1000.0;
    constexpr double cubic_feet = foot * foot * foot;
    constexpr double cubic_inches = inch * inch * inch;
    constexpr double gal = 3.785411784 * L;

    // Molecular
    constexpr double mol = 1.0;
    constexpr double kmol = mol / 1000.0;
    constexpr double mmol = mol / 1000000.0;
    constexpr double lbmol = mol * 453.59237;

    // Flow-rate (moles)
    constexpr double mol_per_sec = mol / sec;
    constexpr double scfm = 0.002641 * lbmol / minute;

    // Area
    constexpr double m2 = 1.0;
    constexpr double cm2 = cm * cm;

    // Pressure
    constexpr double Pa = 1.0;
    constexpr double kPa = Pa * 1000.0;
    constexpr double MPa = Pa * 1000000.0;
    constexpr double atm = 101.325 * kPa;

    constexpr double mbar = Pa * 100.0;
    constexpr double bar = mbar * 1000.0;

    constexpr double psi = lbf / (inch * inch);
    constexpr double psig = psi;
    constexpr double inHg = Pa * 3386.3886666666713;
    constexpr double inH2O = inHg * 0.0734824;

    // Temperature
    constexpr double K = 1.0;
    constexpr double K0 = 273.15;
    constexpr double C = K;
    constexpr double F = (5.0 / 9.0) * K;
    constexpr double F0 = -459.67;

    // Energy
    constexpr double J = 1.0;
    constexpr double kJ = J * 1000;
    constexpr double MJ = J * 1000000;

    // Angles
    constexpr double rad = 1.0;
    constexpr double deg = rad * (constants::pi / 180);

    // Conversions
    inline constexpr double distance(double v, double unit) {
        return v * unit;
    }

    inline constexpr double area(double v, double unit) {
        return v * unit;
    }

    inline constexpr double torque(double v, double unit) {
        return v * unit;
    }

    inline constexpr double rpm(double rpm) {
        return rpm * 0.104719755;
    }

    inline constexpr double toRpm(double rad_s) {
        return rad_s / 0.104719755;
    }

    inline constexpr double pressure(double v, double unit) {
        return v * unit;
    }

    inline constexpr double psia(double p) {
        return units::pressure(p, units::psig) - units::pressure(1.0, units::atm);
    }

    inline constexpr double toPsia(double p) {
        return (p + units::pressure(1.0, units::atm)) / units::psig;
    }

    inline constexpr double mass(double v, double unit) {
        return v * unit;
    }

    inline constexpr double force(double v, double unit) {
        return v * unit;
    }

    inline constexpr double volume(double v, double unit) {
        return v * unit;
    }

    inline constexpr double flow(double v, double unit) {
        return v * unit;
    }

    inline constexpr double convert(double v, double unit0, double unit1) {
        return v * (unit0 / unit1);
    }

    inline constexpr double convert(double v, double unit) {
        return v / unit;
    }

    inline constexpr double celcius(double T_C) {
        return T_C * C + K0;
    }

    inline constexpr double kelvin(double T) {
        return T * K;
    }

    inline constexpr double fahrenheit(double T_F) {
        return F * (T_F - F0);
    }

    inline constexpr double toAbsoluteFahrenheit(double T) {
        return T / F;
    }

    inline constexpr double angle(double v, double unit) {
        return v * unit;
    }

    inline constexpr double energy(double v, double unit) {
        return v * unit;
    }

    // Physical Constants
    constexpr double AirMolecularMass = units::mass(28.97, units::g) / units::mol;
};

#endif